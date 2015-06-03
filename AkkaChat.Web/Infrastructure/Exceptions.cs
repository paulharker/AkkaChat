using System;

namespace AkkaChat.Infrastructure
{
    /// <summary>
    /// Base class for all exceptions related to AkkaChat
    /// </summary>
    public abstract class AkkaChatException : Exception
    {
        protected AkkaChatException(string message) : base(message) { }

        protected AkkaChatException(string message, Exception innerEx) : base(message, innerEx) { }
    }

    /// <summary>
    /// Happens when a user for a deleted account attempts to access the chat
    /// </summary>
    public class UnidentifiedAkkaSessionException : AkkaChatException
    {
        public UnidentifiedAkkaSessionException(string message, Guid sessionId) : base(message)
        {
            SessionId = sessionId;
        }

        public UnidentifiedAkkaSessionException(string message, Exception innerEx, Guid sessionId) : base(message, innerEx)
        {
            SessionId = sessionId;
        }

        public Guid SessionId { get; private set; }
    }

    /// <summary>
    /// Occurs when a session times out
    /// </summary>
    public class AkkaChatSessionTimeoutException : AkkaChatException
    {
        public AkkaChatSessionTimeoutException(string message) : base(message)
        {
        }

        public AkkaChatSessionTimeoutException(string message, Exception innerEx) : base(message, innerEx)
        {
        }
    }
}