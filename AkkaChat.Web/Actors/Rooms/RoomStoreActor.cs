using System;
using Akka.Actor;
using AkkaChat.Data.Repositories;

namespace AkkaChat.Actors
{
    /// <summary>
    /// Responsible for fetching information about chat rooms from the datasore
    /// </summary>
    public class RoomStoreActor : ReceiveActor
    {
        #region Message classes

        public class DoesRoomExist
        {
            public DoesRoomExist(string roomName)
            {
                RoomName = roomName;
            }

            public string RoomName { get; private set; }
        }

        public class RoomExistsResponse
        {
            public RoomExistsResponse(string roomName, bool exists)
            {
                Exists = exists;
                RoomName = roomName;
            }

            public string RoomName { get; private set; }

            public bool Exists { get; private set; }
        }

        public class FetchRoom
        {
            public FetchRoom(string roomName)
            {
                RoomName = roomName;
            }

            public string RoomName { get; private set; }
        }

        #endregion

        private readonly Func<IRoomRepository> _roomRepoFactory;

        public RoomStoreActor(Func<IRoomRepository> roomRepoFactory)
        {
            _roomRepoFactory = roomRepoFactory;

            Receive<DoesRoomExist>(exist =>
            {
                var repo = _roomRepoFactory();
                var sender = Sender;
                var result = repo.RoomExists(exist.RoomName).ContinueWith(tr =>
                {
                    repo.Dispose();
                    if (tr.Result.Payload == true)
                    {
                        return new RoomExistsResponse(exist.RoomName, true);
                    }
                    return new RoomExistsResponse(exist.RoomName, false);
                }).PipeTo(sender);
            });

            Receive<FetchRoom>(async fetchRoom =>
            {
                using (var repo = _roomRepoFactory())
                {
                    var sender = Sender;
                    var room = await repo.FetchRoom(fetchRoom.RoomName);
                    sender.Tell(room.Payload);
                }
            });
        }
    }
}