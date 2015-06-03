using System.Linq;
using Akka.Actor;
using AkkaChat.Hubs;
using AkkaChat.Messages;

namespace AkkaChat.Actors
{
    /// <summary>
    /// Writes OUTBOUND messages sent from the server to
    /// client-side listeners to a SignalR <see cref="ChatHub"/>
    /// 
    /// Receives messages from a <see cref="RoomManagerActor"/>
    /// </summary>
    public class SignalrWriter : ReceiveActor, IWithUnboundedStash
    {
        #region Message types

        public class SetHub
        {
            public SetHub(ChatHub hub)
            {
                Hub = hub;
            }

            public ChatHub Hub { get; private set; }
        }

        #endregion

        private ChatHub _hub;

        public SignalrWriter()
        {
            WaitingForHub();
        }

        private void WaitingForHub()
        {
            Receive<SetHub>(hub =>
            {
                _hub = hub.Hub;
                BecomeReady();
            });
            
            ReceiveAny(o => Stash.Stash());
        }

        private void BecomeReady()
        {
            Stash.UnstashAll();
            Become(Ready);
        }

        private void Ready()
        {
            Receive<SetHub>(hub => { }); //ignore

            Receive<ChatMessage>(message => _hub.SendToRoom(message.RoomName, new MessageViewModel(message)));

            Receive<UserJoined>(joined =>
            {
                _hub.AddUserToRoom(joined.RoomName, new UserViewModel(joined.User.DisplayName));
            });

            Receive<UserDisconnected>(disconnected =>
            {
                _hub.RemoveUserFromRoom(disconnected.RoomName, new UserViewModel(disconnected.User.DisplayName));
            });

            Receive<HistoricalMessageBatch>(messages =>
            {
                foreach (var message in messages.Messages.OrderBy(x => x.When))
                {
                    _hub.CatchUpUser(messages.ConnectionId, messages.RoomName, new MessageViewModel(message));
                }
            });
        }


        public IStash Stash { get; set; }
    }
}