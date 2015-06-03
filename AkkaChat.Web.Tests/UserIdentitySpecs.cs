using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Akka.Actor;
using Akka.TestKit.NUnit;
using AkkaChat.Actors;
using AkkaChat.Data.Models;
using AkkaChat.Data.Repositories;
using AkkaChat.Data.Repositories.SQL;
using AkkaChat.Messages;
using Faker;
using Microsoft.CSharp;
using NUnit.Framework;

namespace AkkaChat.Web.Tests
{
    [TestFixture]
    public class UserIdentitySpecs : TestKit
    {
        private Func<IUserRepository> _userRepoFunction;
        private Func<IdentityStoreActor.CheckForAvailableUserName> _checkUserNameFunc = () => new IdentityStoreActor.CheckForAvailableUserName(Faker.Generators.Names.FullName());
        private Fake<User> _fakeUser = new Fake<User>();

        public UserIdentitySpecs()
        {
            _userRepoFunction = () => new SqlUserRepository(DbSupport.Queries, new SqlConnection(DbSupport.ConnectionString));
        }

        [Test]
        public void IdentityStoreActor_should_use_new_repository_for_each_request()
        {
            var callCount = 0;
            Func<IUserRepository> generatorWithCallCount = () =>
            {
                callCount++; //wrap the original _userRepoFunction with call count behavior
                return _userRepoFunction();
            };
            var identityActor = Sys.ActorOf(Props.Create(() => new IdentityStoreActor(generatorWithCallCount)));
            var userNameRequest = _checkUserNameFunc();
            var expectedCalls = 4;
            foreach (var call in Enumerable.Repeat(0, expectedCalls))
            {
                identityActor.Tell(userNameRequest);
                Assert.True(ExpectMsg<IdentityStoreActor.UserNameAvailablity>().IsAvailable);
            }
                
            Assert.AreEqual(expectedCalls, callCount);
        }

        [Test]
        public void IdentityStoreActor_should_not_find_nonexistent_user()
        {
            var identityActor = Sys.ActorOf(Props.Create(() => new IdentityStoreActor(_userRepoFunction)));

            //generate a random username request
            var userNameRequest = _checkUserNameFunc();
            identityActor.Tell(userNameRequest);
            var available = ExpectMsg<IdentityStoreActor.UserNameAvailablity>().IsAvailable;
            Assert.True(available);
        }

        [Test]
        public void IdentityStoreActor_should_handle_successive_requests_without_issue()
        {
            var identityActor = Sys.ActorOf(Props.Create(() => new IdentityStoreActor(_userRepoFunction)));
            var requests = new List<IdentityStoreActor.CheckForAvailableUserName>();
            foreach (var i in Enumerable.Repeat(1,4))
            {
                var r = _checkUserNameFunc();
                identityActor.Tell(r);
                requests.Add(r);
            }

            foreach (var i in Enumerable.Repeat(1, 4))
            {
                var m = ExpectMsg<IdentityStoreActor.UserNameAvailablity>();
                Assert.True(requests.Count(x => x.DisplayName.Equals(m.DisplayName)) == 1, "Should be exactly 1 request with this display name");
                Assert.True(m.IsAvailable, "All of these names should be available");
            }
        }


        [Test]
        public void IdentityStoreActor_should_fetch_existing_user()
        {
            var identityActor = Sys.ActorOf(Props.Create(() => new IdentityStoreActor(_userRepoFunction)));
            var identityWriterActor = Sys.ActorOf(Props.Create(() => new IdentityWriterActor(_userRepoFunction())));
            var user = _fakeUser.Generate(); //generate a user instance

            identityWriterActor.Tell(new IdentityWriterActor.CreateUser(user.DisplayName, user.Email, user.PasswordHash, user.PasswordSalt, user.DateJoined, user.CookieId));
            var createResult = ExpectMsg<IdentityWriterActor.UserCreateResult>();
            Assert.True(createResult.Success);

            //query the identity actor
            AwaitAssert(() =>
            {
                identityActor.Tell(new IdentityStoreActor.FetchUserByName(user.DisplayName));
                var actualUser = ExpectMsg<UserIdentity>();
                Assert.IsNotInstanceOf<MissingUserIdentity>(actualUser);
                Assert.AreEqual(user.Email, actualUser.Email);
                Assert.AreEqual(user.CookieId, actualUser.CookieId);
            }, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(30));
          
        }

        [Test]
        public void IdentityStoreActor_should_fetch_authentication_information_for_existing_user()
        {
            var identityActor = Sys.ActorOf(Props.Create(() => new IdentityStoreActor(_userRepoFunction)));
            var identityWriterActor = Sys.ActorOf(Props.Create(() => new IdentityWriterActor(_userRepoFunction())));
            var user = _fakeUser.Generate(); //generate a user instance

            identityWriterActor.Tell(new IdentityWriterActor.CreateUser(user.DisplayName, user.Email, user.PasswordHash, user.PasswordSalt, user.DateJoined, user.CookieId));
            var createResult = ExpectMsg<IdentityWriterActor.UserCreateResult>();
            Assert.True(createResult.Success);

            //query the identity actor for AUTHORIZATION information
            AwaitAssert(() =>
            {
                identityActor.Tell(new IdentityStoreActor.FetchAuthenticationInformation(user.DisplayName));
                var actualUser = ExpectMsg<AuthenticationInformation>();
                Assert.IsNotInstanceOf<MissingAuthenticationInformation>(actualUser);
                Assert.AreEqual(user.PasswordHash, actualUser.HashedPassword);
            }, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(30));
        }

        [Test]
        public void IdentityStoreActor_should_fetch_information_for_existing_user_by_CookieId()
        {
            var identityActor = Sys.ActorOf(Props.Create(() => new IdentityStoreActor(_userRepoFunction)));
            var identityWriterActor = Sys.ActorOf(Props.Create(() => new IdentityWriterActor(_userRepoFunction())));
            var user = _fakeUser.Generate(); //generate a user instance

            identityWriterActor.Tell(new IdentityWriterActor.CreateUser(user.DisplayName, user.Email, user.PasswordHash, user.PasswordSalt, user.DateJoined, user.CookieId));
            var createResult = ExpectMsg<IdentityWriterActor.UserCreateResult>();
            Assert.True(createResult.Success);

            //query the identity actor for AUTHORIZATION information
            AwaitAssert(() =>
            {
                identityActor.Tell(new IdentityStoreActor.FetchUserByCookie(user.CookieId));
                var actualUser = ExpectMsg<UserIdentity>();
                Assert.IsNotInstanceOf<MissingUserIdentity>(actualUser);
                Assert.AreEqual(user.DisplayName, actualUser.DisplayName);
            }, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(30));
        }

        [Test]
        public void IdentityWriterActor_should_be_able_to_write_nonexistent_users()
        {
            var identityWriterActor = Sys.ActorOf(Props.Create(() => new IdentityWriterActor(_userRepoFunction())));
            var user = _fakeUser.Generate(); //generate a user instance

            identityWriterActor.Tell(new IdentityWriterActor.CreateUser(user.DisplayName, user.Email, user.PasswordHash, user.PasswordSalt, user.DateJoined, user.CookieId));
            var createResult = ExpectMsg<IdentityWriterActor.UserCreateResult>();
            Assert.True(createResult.Success);
        }

        [Test]
        public void IdentityWriterActor_should_not_be_able_to_overwrite_existing_user()
        {
            var writerActor1 = Sys.ActorOf(Props.Create(() => new IdentityWriterActor(_userRepoFunction())));
            var user = _fakeUser.Generate(); //generate a user instance

            writerActor1.Tell(new IdentityWriterActor.CreateUser(user.DisplayName, user.Email, user.PasswordHash, user.PasswordSalt, user.DateJoined, user.CookieId));
            var createResult = ExpectMsg<IdentityWriterActor.UserCreateResult>();
            Assert.True(createResult.Success);

            //attempt to recreate the same user
            var writerActor2 = Sys.ActorOf(Props.Create(() => new IdentityWriterActor(_userRepoFunction())));
            writerActor2.Tell(new IdentityWriterActor.CreateUser(user.DisplayName, user.Email, user.PasswordHash, user.PasswordSalt, user.DateJoined, user.CookieId));
            createResult = ExpectMsg<IdentityWriterActor.UserCreateResult>();
            Assert.False(createResult.Success);
        }
    }
}
