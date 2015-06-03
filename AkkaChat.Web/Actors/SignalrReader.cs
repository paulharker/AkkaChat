using System;
using Akka.Actor;
using AkkaChat.Hubs;
using AkkaChat.Messages;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace AkkaChat.Actors
{
    /// <summary>
    /// Reads INCOMING messages sent from the clients to a 
    /// server-side SignalR <see cref="ChatHub"/>.
    /// 
    /// Forwards these messages onto the appropriate <see cref="RoomManagerActor"/>
    /// </summary>
    public class SignalrReader : ReceiveActor
    {
        #region Message types

        public class AuthenticatedUserConnect
        {
            public AuthenticatedUserConnect(string userName, string roomName, DateTime when)
            {
                When = when;
                RoomName = roomName;
                UserName = userName;
            }

            public string UserName { get; private set; }

            public string RoomName { get; private set; }

            public DateTime When { get; private set; }
        }

        public class AuthenticatedUserDisconnect
        {
            public AuthenticatedUserDisconnect(string userName, string roomName, DateTime when)
            {
                When = when;
                RoomName = roomName;
                UserName = userName;
            }

            public string UserName { get; private set; }

            public string RoomName { get; private set; }

            public DateTime When { get; private set; }
        }

        #endregion

        private readonly IActorRef _roomMaster;
        private readonly IActorRef _sessionManager;

        public SignalrReader(IActorRef roomMaster, IActorRef sessionManager)
        {
            _roomMaster = roomMaster;
            _sessionManager = sessionManager;

            Receive<AuthenticatedUserConnect>(connect =>
            {
                var self = Context.Self;
                sessionManager.Ask<UserIdentity>(new UserSessionActor.FetchIdentity(connect.UserName))
                    .ContinueWith(tr =>
                    {
                        if (!(tr.Result is MissingUserIdentity))
                        {
                            _roomMaster.Tell(new UserJoined(tr.Result, connect.RoomName, connect.When));
                        }
                    });
            });

            Receive<AuthenticatedUserDisconnect>(disconnect =>
            {
                sessionManager.Ask<UserIdentity>(new UserSessionActor.FetchIdentity(disconnect.UserName))
                    .ContinueWith(tr =>
                    {
                        if (!(tr.Result is MissingUserIdentity))
                        {
                            _sessionManager.Tell(new UserDisconnected(tr.Result, disconnect.RoomName, disconnect.When));
                        }
                    });
            });

            Receive<FetchPreviousMessages>(messages => _roomMaster.Tell(messages));

            Receive<UserMessage>(message => _roomMaster.Tell(message));
        }
    }
}