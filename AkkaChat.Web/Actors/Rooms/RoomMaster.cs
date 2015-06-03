using System.Collections.Generic;
using Akka.Actor;
using Akka.DI.Core;
using Akka.Event;
using AkkaChat.Data.Models;
using AkkaChat.Infrastructure;
using AkkaChat.Messages;

namespace AkkaChat.Actors
{
    /// <summary>
    /// Actor responsible for managing the state of all rooms
    /// </summary>
    public class RoomMaster : ReceiveActor
    {
        #region Message classes

        public class GetRoomActor
        {
            public GetRoomActor(string roomName)
            {
                RoomName = roomName;
            }

            public string RoomName { get; private set; }
        }

        public class RoomActorPair
        {
            public RoomActorPair(ExistingRoom room, IActorRef roomManager)
            {
                RoomManager = roomManager;
                Room = room;
            }

            public ExistingRoom Room { get; private set; }

            public IActorRef RoomManager { get; private set; }
        }

        #endregion

        private readonly IActorRef _signalRWriter;
        private IActorRef _roomStore;
        private ILoggingAdapter _log = Context.GetLogger();
        private readonly IDictionary<string, RoomActorPair> _rooms;


        public RoomMaster(IActorRef signalRWriter)
        {
            _signalRWriter = signalRWriter;
            _rooms = new Dictionary<string, RoomActorPair>();
            Receive<MissingRoom>(missing =>
            {
                _log.Warning("Received MissingRoom notification for {0} - you need to create it first!",
                    missing.DisplayName);
            });

            Receive<ExistingRoom>(room =>
            {
                _log.Info("Created chatroom {0}", room.DisplayName);
                var roomManager = CreateRoom(room);
                _rooms[room.DisplayName] = new RoomActorPair(room, roomManager);
            });

            Receive<GetRoomActor>(room =>
            {
                if (_rooms.ContainsKey(room.RoomName))
                {
                    Sender.Tell(_rooms[room.RoomName]);
                }
                else
                {
                    Unhandled(room);
                }
            });

            //generic message handling
            Receive<IRouteToRoom>(msg =>
            {
                if (_rooms.ContainsKey(msg.RoomName))
                {
                    _rooms[msg.RoomName].RoomManager.Forward(msg);
                }
            });
        }

        protected override void PreStart()
        {
            _roomStore = Context.DI().ActorOf<RoomStoreActor>("store");

            /* Pre-Warm the default chat room */
            _roomStore.Tell(new RoomStoreActor.FetchRoom(Constants.DefaultChatRoom));
        }

        private IActorRef CreateRoom(ExistingRoom roomId)
        {
            return Context.ActorOf(Props.Create(() => new RoomManagerActor(roomId, _signalRWriter)), string.Format("rooms-{0}",roomId.Id));
        }
    }
}