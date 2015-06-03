using System;

namespace AkkaChat.Data.Models
{
    /// <summary>
    /// User entity
    /// </summary>
    public class User
    {
        public string DisplayName { get; set; }

        public string Email { get; set; }

        public Guid CookieId { get; set; }

        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

        public DateTime DateJoined { get; set; }
    }

    /// <summary>
    /// Entity mapping for an existing user
    /// </summary>
    public class ExistingUser : User 
    {
        public int UserId { get; set; }
    }

    /// <summary>
    /// Special case pattern for missing users
    /// </summary>
    public class MissingUser : ExistingUser { }
}
