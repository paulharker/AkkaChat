using System.Collections.Generic;
using AkkaChat.Messages;
using Nancy.Security;

namespace AkkaChat.Infrastructure
{
    /// <summary>
    /// Nancy User Identity
    /// </summary>
    public class AkkaChatIdentity : IUserIdentity
    {
        private static readonly List<string> AllClaims = new List<string>(){ "All" };

        public AkkaChatIdentity(UserIdentity identity)
        {
            Identity = identity;
        }

        public UserIdentity Identity { get; private set; }

        public string UserName { get { return Identity.DisplayName; }}

        public IEnumerable<string> Claims { get { return AllClaims; } }
    }
}