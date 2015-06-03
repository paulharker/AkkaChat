using System;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaChat.Actors;
using AkkaChat.Data.Models;
using AkkaChat.Infrastructure;
using AkkaChat.Messages;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace AkkaChat.Hubs
{
    [HubName("chatHub")]
    public class ChatHub : Hub
    {
        private bool _setHub = false;

        public override Task OnConnected()
        {
            if (!_setHub)
            {
                ActorSystemRefs.SignalRWriter.Tell(new SignalrWriter.SetHub(this));
                _setHub = true;
            }
            
            ActorSystemRefs.SignalRReader.Tell(new FetchPreviousMessages(Context.ConnectionId, Constants.DefaultChatRoom));
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            /*
            * Does not work, due to Nancy / Owin / SignalR not actually working very well together
            */
            if (Context.User.Identity.IsAuthenticated)
            {
                ActorSystemRefs.SignalRReader.Tell(new SignalrReader.AuthenticatedUserDisconnect(Context.User.Identity.Name, Constants.DefaultChatRoom, DateTime.Now));
            }
            return base.OnDisconnected(stopCalled);
        }

        public void Send(ClientMessage message)
        {
            if (message.IsValid())
            {
         
                ActorSystemRefs.SignalRReader.Tell(new UserMessage(Guid.NewGuid().ToString(), message.Content,
                    DateTime.Now, message.RoomName, message.UserName));
            }
            else
            {
                /* 
                  Could add some warning for invalid input here
                 */
            }
         }

        public Task JoinRoom(string roomName, string userName)
        {
            ActorSystemRefs.SignalRReader.Tell(new SignalrReader.AuthenticatedUserConnect(userName, Constants.DefaultChatRoom, DateTime.Now));
            return Groups.Add(Context.ConnectionId, roomName);
        }

        public Task LeaveRoom(string roomName, string userName)
        {
            ActorSystemRefs.SignalRReader.Tell(new SignalrReader.AuthenticatedUserDisconnect(userName, Constants.DefaultChatRoom, DateTime.Now));
            return Groups.Remove(Context.ConnectionId, roomName);
        }

        /// <summary>
        /// Only gets called directly by our SignalRWriterActor.
        /// </summary>
        /// <param name="roomName">The name of the room to write to.</param>
        /// <param name="message">The message to write.</param>
        internal void SendToRoom(string roomName, MessageViewModel message)
        {
            Clients.Group(roomName).addMessage(roomName, message);
        }

        internal void AddUserToRoom(string roomName, UserViewModel user)
        {
            Clients.Group(roomName).addUser(roomName, user);
        }

        internal void RemoveUserFromRoom(string roomName, UserViewModel user)
        {
            Clients.Group(roomName).removeUser(roomName, user);
        }

        internal void CatchUpUser(string connectionId, string roomName, MessageViewModel message)
        {
            Clients.Client(connectionId).addMessage(roomName, message);
        }
    }
}