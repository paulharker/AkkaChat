using System;
using System.Collections.Generic;

namespace AkkaChat.Messages
{
    /// <summary>
    /// Interface used to route messages to specific chatrooms
    /// </summary>
    public interface IRouteToRoom
    {
        string RoomName { get; }
    }

    /// <summary>
    /// Broadcast message when a user joins the room
    /// </summary>
    public class UserJoined : IRouteToRoom
    {
        public UserJoined(UserIdentity user, string roomName, DateTime when)
        {
            When = when;
            RoomName = roomName;
            User = user;
        }

        public UserIdentity User { get; private set; }

        public string RoomName { get; private set; }

        public DateTime When { get; private set; }
    }

    /// <summary>
    /// Broadcast message when a user leaves the room
    /// </summary>
    public class UserDisconnected : IRouteToRoom
    {
        public UserDisconnected(UserIdentity user, string roomName, DateTime when)
        {
            When = when;
            RoomName = roomName;
            User = user;
        }

        public UserIdentity User { get; private set; }

        public string RoomName { get; private set; }

        public DateTime When { get; private set; }
    }

    public class ChatMessage : IComparable<ChatMessage>, IRouteToRoom
    {
        public ChatMessage(string id, string message, DateTime when, string roomName, string userName)
        {
            UserName = userName;
            RoomName = roomName;
            When = when;
            Message = message;
            Id = id;
        }

        public string Id { get; private set; }

        public string UserName { get; private set; }

        public string RoomName { get; private set; }

        public string Message { get; private set; }

        public DateTime When { get; private set; }

        public int CompareTo(ChatMessage other)
        {
            return When.CompareTo(other.When);
        }

        protected bool Equals(ChatMessage other)
        {
            return string.Equals(Id, other.Id) && When.Equals(other.When) && string.Equals(RoomName, other.RoomName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ChatMessage) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ When.GetHashCode();
                hashCode = (hashCode*397) ^ (RoomName != null ? RoomName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public class SystemMessage : ChatMessage
    {
        public const string SystemUserName = "System";

        public SystemMessage(string id, string message, DateTime when, string roomName) : base(id, message, when, roomName, SystemUserName)
        {
        }
    }

    public class UserMessage : ChatMessage
    {
        public UserMessage(string id, string message, DateTime when, string roomName, string userName) : base(id, message, when, roomName, userName)
        {
        }
    }

    /// <summary>
    /// Request to retrieve previous messages
    /// </summary>
    public class FetchPreviousMessages : IRouteToRoom
    {
        public FetchPreviousMessages(string connectionId, string roomName, DateTime? since = null)
        {
            Since = since;
            RoomName = roomName;
            ConnectionId = connectionId;
        }

        /// <summary>
        /// The connectionId of the client requesting the history of messages
        /// </summary>
        public string ConnectionId { get; private set; }

        public string RoomName { get; private set; }

        public DateTime? Since { get; private set; }
    }

    /// <summary>
    /// Used to help a client recieve the previous history of messages from the chatroom
    /// </summary>
    public class HistoricalMessageBatch : IRouteToRoom
    {
        public HistoricalMessageBatch(IEnumerable<ChatMessage> messages, string roomName, string connectionId)
        {
            ConnectionId = connectionId;
            RoomName = roomName;
            Messages = messages;
        }

        public string ConnectionId { get; private set; }

        public IEnumerable<ChatMessage> Messages { get; private set; }

        public string RoomName { get; private set; }
    }
}