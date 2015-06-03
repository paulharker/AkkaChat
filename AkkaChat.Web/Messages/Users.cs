using System;

namespace AkkaChat.Messages
{
    /// <summary>
    /// Represents the identity of an authenticated user
    /// </summary>
    public class UserIdentity
    {
        public UserIdentity(int userId, string displayName, string email, DateTime dateJoined, Guid cookieId)
        {
            CookieId = cookieId;
            DateJoined = dateJoined;
            Email = email;
            DisplayName = displayName;
            UserId = userId;
        }

        public int UserId { get; private set; }

        public string DisplayName { get; private set; }

        public Guid CookieId { get; private set; }

        public string Email { get; private set; }

        public DateTime DateJoined { get; private set; }
    }

    /// <summary>
    /// SPECIAL CASE PATTERN.
    /// 
    /// Used in instances where we were unable to lookup the requested user.
    /// </summary>
    public class MissingUserIdentity : UserIdentity {
        public MissingUserIdentity(string displayName) : base(-1, displayName, string.Empty, DateTime.MinValue, Guid.Empty)
        {
        }
    }
}