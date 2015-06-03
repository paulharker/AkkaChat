using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.TestKit.NUnit;
using Akka.TestKit.TestActors;
using AkkaChat.Actors;
using AkkaChat.Data.Models;
using AkkaChat.Data.Repositories;
using AkkaChat.Data.Repositories.SQL;
using AkkaChat.Messages;
using Faker;
using MVC.Utilities.Encryption;
using NUnit.Framework;

namespace AkkaChat.Web.Tests
{
    [TestFixture]
    public class AuthChallengeSpecs : TestKit
    {
        private Func<IUserRepository> _userRepoFunction;
        private Func<IdentityStoreActor.CheckForAvailableUserName> _checkUserNameFunc = () => new IdentityStoreActor.CheckForAvailableUserName(Faker.Generators.Names.FullName());
        private Fake<User> _fakeUser = new Fake<User>();
        private ICryptoService _cryptoService = new HMACSHA1Service(Guid.NewGuid().ToString());

        public AuthChallengeSpecs() : base("akka.test.single-expect-default = 5s")
        {
            _userRepoFunction = () => new SqlUserRepository(DbSupport.Queries, new SqlConnection(DbSupport.ConnectionString));
        }

        [Test]
        public void AuthChallengeActor_should_fail_challenge_on_timeout()
        {
            var blackhole = Sys.ActorOf(BlackHoleActor.Props);
            var auth = Sys.ActorOf(Props.Create(() => new AuthenticationChallengeActor(_cryptoService, TimeSpan.FromSeconds(3), blackhole)));
            auth.Tell(new AuthenticationChallenge("Test", "test"));
            var fail = ExpectMsg<AuthenticationFailure>();
            Assert.AreEqual("Test", fail.DisplayName);
        }

        [Test]
        public void AuthChallengeActor_should_fail_challenge_on_missing_user()
        {
            var identity = GetIdentityStoreActor();
            var auth = Sys.ActorOf(Props.Create(() => new AuthenticationChallengeActor(_cryptoService, TimeSpan.FromSeconds(3), identity)));

            auth.Tell(new AuthenticationChallenge("Test", "test"));
            var fail = ExpectMsg<AuthenticationFailure>();
            Assert.AreEqual("Test", fail.DisplayName);
        }

        [Test]
        public async Task AuthChallengeActor_should_fail_challenge_for_existing_user_bad_password()
        {
            var writer = GetIdentityWriterActor();
            var identity = GetIdentityStoreActor();
            var auth = Sys.ActorOf(Props.Create(() => new AuthenticationChallengeActor(_cryptoService, TimeSpan.FromSeconds(3), identity)));

            var user = _fakeUser.Generate();
            var originalPassword = user.PasswordHash;
            user.PasswordHash = _cryptoService.HashPassword(originalPassword, user.PasswordSalt);

            //need to await so we can confirm that the create operation has succeeded before challenging
            var result = await writer.Ask<IdentityWriterActor.UserCreateResult>(
                new IdentityWriterActor.CreateUser(user.DisplayName, user.Email, user.PasswordHash, user.PasswordSalt, user.DateJoined, user.CookieId));

            auth.Tell(new AuthenticationChallenge(user.DisplayName, originalPassword + "foo"));
            var fail = ExpectMsg<AuthenticationFailure>();
            Assert.AreEqual(user.DisplayName, fail.DisplayName);
        }

        [Test]
        public async Task AuthChallengeActor_should_pass_challenge_for_existing_user_correct_password()
        {
            var writer = GetIdentityWriterActor();
            var identity = GetIdentityStoreActor();
            var auth = Sys.ActorOf(Props.Create(() => new AuthenticationChallengeActor(_cryptoService, TimeSpan.FromSeconds(3), identity)));

            var user = _fakeUser.Generate();
            var originalPassword = user.PasswordHash;
            user.PasswordHash = _cryptoService.HashPassword(originalPassword, user.PasswordSalt);

            //need to await so we can confirm that the create operation has succeeded before challenging
            var result = await writer.Ask<IdentityWriterActor.UserCreateResult>(
                new IdentityWriterActor.CreateUser(user.DisplayName, user.Email, user.PasswordHash, user.PasswordSalt, user.DateJoined, user.CookieId));


            auth.Tell(new AuthenticationChallenge(user.DisplayName, originalPassword));
            var pass = ExpectMsg<AuthenticationSuccess>();
            Assert.AreEqual(user.DisplayName, pass.User.DisplayName);
        }

        private IActorRef GetIdentityStoreActor()
        {
            return Sys.ActorOf(Props.Create(() => new IdentityStoreActor(_userRepoFunction)));
        }

        private IActorRef GetIdentityWriterActor()
        {
            return Sys.ActorOf(Props.Create(() => new IdentityWriterActor(_userRepoFunction())));
        }
    }
}
