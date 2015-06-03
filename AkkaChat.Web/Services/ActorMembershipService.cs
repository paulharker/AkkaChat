using System;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaChat.Actors;
using AkkaChat.Messages;
using MVC.Utilities.Encryption;

namespace AkkaChat.Services
{
    /// <summary>
    /// Actor-based membership provider
    /// </summary>
    public class ActorMembershipService : IMembershipService
    {
        private readonly ICryptoService _cryptoService;
        private readonly IActorRef _identityManager;
        private readonly IActorRef _sessionManager;

        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(3);

        protected string SaltGenerator()
        {
            return Guid.NewGuid().ToString();
        }

        public ActorMembershipService(ICryptoService cryptoService, IActorRef identityManager, IActorRef sessionManager)
        {
            _identityManager = identityManager;
            _sessionManager = sessionManager;
            _cryptoService = cryptoService;
        }

        public async Task<UserIdentity> AddUser(string userName, string email, string password)
        {
            var salt = SaltGenerator();
            var hashedPassword = _cryptoService.HashPassword(password, salt);

            var createUser = new IdentityWriterActor.CreateUser(userName, email, hashedPassword, salt, DateTime.Now, Guid.NewGuid());
            var createResult = await _identityManager.Ask<IdentityWriterActor.UserCreateResult>(createUser, DefaultTimeout);
            if (createResult.Success)
            {
                return await _identityManager.Ask<UserIdentity>(new IdentityStoreActor.FetchUserByName(userName));
            }
            else
            {
                return new MissingUserIdentity(userName);
            }
        }

        public async Task<bool> IsUserNameAvailable(string userName)
        {
            var result = await _identityManager.Ask<IdentityStoreActor.UserNameAvailablity>(
                new IdentityStoreActor.CheckForAvailableUserName(userName), DefaultTimeout);

            return result.IsAvailable;
        }

        public bool TryAuthenticateUser(string userName, string password, out AuthenticationSuccess user)
        {
            var authChallenge = new AuthenticationChallenge(userName, password);
            var task = _sessionManager.Ask<object>(authChallenge, DefaultTimeout);
            task.Wait();

            user = task.Result as AuthenticationSuccess;
            if (user == null) return false;
            return true;
        }

        public async Task<UserIdentity> FetchUser(string userName)
        {
            return await _identityManager.Ask<UserIdentity>(new IdentityStoreActor.FetchUserByName(userName), DefaultTimeout);
        }
    }
}