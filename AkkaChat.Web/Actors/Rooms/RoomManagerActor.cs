using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Util.Internal;
using AkkaChat.Data.Models;
using AkkaChat.Messages;

namespace AkkaChat.Actors
{
    /// <summary>
    /// Formats AkkaChat events into pretty system messages
    /// </summary>
    public static class SystemMessageFormatters
    {
        public static SystemMessage ToSystemMessage(this UserJoined userJoined, ExistingRoom room)
        {
            return new SystemMessage(Guid.NewGuid().ToString(),
                String.Format("{0} has JOINED {1}", userJoined.User.DisplayName, room.DisplayName), userJoined.When, room.DisplayName);
        }

        public static SystemMessage ToSystemMessage(this UserDisconnected userDisconnected, ExistingRoom room)
        {
            return new SystemMessage(Guid.NewGuid().ToString(),
                String.Format("{0} has DISCONNECTED {1}", userDisconnected.User.DisplayName, room.DisplayName),
                userDisconnected.When, room.DisplayName);

        }
    }

    /// <summary>
    /// Top-level actor responsible for routing of messages between SignalR hubs and our stateful actors.
    /// 
    /// Doesn't do much aside from reboot its children and route messages back to SignalR.
    /// </summary>
    public class RoomManagerActor : ReceiveActor
    {
        private readonly ExistingRoom _room;
        private HashSet<UserIdentity> _users;
        private IActorRef _signalRWriter;
        private IActorRef _messageHistory;

        public RoomManagerActor(ExistingRoom room, IActorRef signalRWriter)
        {
            _room = room;
            _signalRWriter = signalRWriter;
            _users = new HashSet<UserIdentity>();

            Receive<UserJoined>(user =>
            {
                if (_users.Add(user.User))
                {
                    _signalRWriter.Tell(user); //forward this message onto the SignalR writer
                    _messageHistory.Tell(user.ToSystemMessage(_room)); //add a system message to the history
                }
            });

            Receive<UserDisconnected>(user =>
            {
                if (_users.Remove(user.User))
                {
                    _signalRWriter.Tell(user);  //forward this message onto the SignalR writer
                    _messageHistory.Tell(user.ToSystemMessage(_room)); //add a system message to the history
                }
            });

            // pass any chat messages onto the RoomMessageHistoryActor
            // but make us the sender of the message
            Receive<ChatMessage>(message =>
            {
                _messageHistory.Tell(message);
            });

            Receive<FetchPreviousMessages>(fetch =>
            {
                _messageHistory.Tell(fetch);
            });
        }

        protected override void PreStart()
        {
            _messageHistory = Context.ActorOf(Props.Create(() => new RoomMessageHistoryActor(_room, _signalRWriter)),
                "history");
        }

        protected override void PreRestart(Exception reason, object message)
        {
            //don't kill our children
        }
    }
}