using System;

namespace AkkaChat.Messages
{
    public sealed class AuthenticationChallenge : IEquatable<AuthenticationChallenge>
    {
        public AuthenticationChallenge(string displayName, string attemptedPassword)
        {
            AttemptedPassword = attemptedPassword;
            DisplayName = displayName;
        }

        public string DisplayName { get; private set; }

        public string AttemptedPassword { get; private set; }

        public bool Equals(AuthenticationChallenge other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(DisplayName, other.DisplayName) && string.Equals(AttemptedPassword, other.AttemptedPassword);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AuthenticationChallenge)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((DisplayName != null ? DisplayName.GetHashCode() : 0) * 397) ^ (AttemptedPassword != null ? AttemptedPassword.GetHashCode() : 0);
            }
        }
    }

    public sealed class AuthenticationChallengeHash : IEquatable<AuthenticationChallengeHash>
    {
        public AuthenticationChallengeHash(string displayName, string attemptedPasswordHash)
        {
            AttemptedPasswordHash = attemptedPasswordHash;
            DisplayName = displayName;
        }

        public string DisplayName { get; private set; }

        public string AttemptedPasswordHash { get; private set; }

        public bool Equals(AuthenticationChallengeHash other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(DisplayName, other.DisplayName) && string.Equals(AttemptedPasswordHash, other.AttemptedPasswordHash);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AuthenticationChallengeHash)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((DisplayName != null ? DisplayName.GetHashCode() : 0) * 397) ^ (AttemptedPasswordHash != null ? AttemptedPasswordHash.GetHashCode() : 0);
            }
        }
    }

    public class AuthenticationInformation
    {
        public AuthenticationInformation(string displayName, string hashedPassword, string salt)
        {
            Salt = salt;
            HashedPassword = hashedPassword;
            DisplayName = displayName;
        }

        public string DisplayName { get; private set; }

        public string HashedPassword { get; private set; }

        public string Salt { get; private set; }
    }

    /// <summary>
    /// SPECIAL CASE PATTERN.
    /// 
    /// Instances where a user attempted to authenticate, but we have no record
    /// of this user in the database.
    /// </summary>
    public class MissingAuthenticationInformation : AuthenticationInformation
    {
        public MissingAuthenticationInformation(string displayName)
            : base(displayName, string.Empty, string.Empty)
        {
        }
    }

    public class AuthenticationSuccess
    {
        public AuthenticationSuccess(UserIdentity user, Guid cookieId)
        {
            CookieId = cookieId;
            User = user;
        }

        public UserIdentity User { get; private set; }

        public Guid CookieId { get; private set; }
    }

    public class AuthenticationFailure
    {
        public AuthenticationFailure(string displayName, string reason)
        {
            Reason = reason;
            DisplayName = displayName;
        }

        public string DisplayName { get; private set; }

        public string Reason { get; private set; }
    }
}