using System;
using Akka.Actor;
using AkkaChat.Data.Models;
using AkkaChat.Data.Repositories;
using AkkaChat.Messages;

namespace AkkaChat.Actors
{
    /// <summary>
    /// Actor reponsible for fetching users from the local system
    /// </summary>
    public class IdentityStoreActor : ReceiveActor
    {
        #region Message classes

        public class CheckForAvailableUserName
        {
            public CheckForAvailableUserName(string displayName)
            {
                DisplayName = displayName;
            }

            public string DisplayName { get; private set; }
        }

        public class UserNameAvailablity
        {
            public UserNameAvailablity(string displayName, bool isAvailable)
            {
                IsAvailable = isAvailable;
                DisplayName = displayName;
            }

            public string DisplayName { get; private set; }

            public bool IsAvailable { get; private set; }
        }

        public class FetchAuthenticationInformation
        {
            public FetchAuthenticationInformation(string displayName)
            {
                DisplayName = displayName;
            }

            public string DisplayName { get; private set; }
        }

        public class FetchUserByName
        {
            public FetchUserByName(string displayName)
            {
                DisplayName = displayName;
            }

            public string DisplayName { get; private set; }
        }

        public class FetchUserByCookie
        {
            public FetchUserByCookie(Guid cookieId)
            {
                CookieId = cookieId;
            }

            public Guid CookieId { get; private set; }
        }

        #endregion

        private readonly Func<IUserRepository> _userRepoFactory;

        public IdentityStoreActor(Func<IUserRepository> userRepoFactory)
        {
            _userRepoFactory = userRepoFactory;

            Receive<CheckForAvailableUserName>(name =>
            {
                var repo = _userRepoFactory();
                var sender = Sender;
                var result = repo.UserNameIsAvailable(name.DisplayName).ContinueWith(tr =>
                {
                    repo.Dispose();
                    if (tr.Result.Payload == true)
                    {
                        return new UserNameAvailablity(name.DisplayName, true);
                    }
                    return new UserNameAvailablity(name.DisplayName, false);
                }).PipeTo(sender);
            });

            /*
             * Used ASYNC / AWAIT here to illustrate how to do it.
             * 
             * It's generally recommended to use PipeTo since it makes it
             * easier to reason about actor code and doesn't require
             * suspending the mailbox in order to process multiple asynchronous
             * operations concurrently.
             */
            Receive<FetchAuthenticationInformation>(async information =>
            {
                using (var repo = _userRepoFactory())
                {
                    var sender = Sender;
                    var user = await repo.FetchUser(information.DisplayName);
                    if (user.Status == ResultCode.Success && !(user.Payload is MissingUser)) //successful fetch
                    {
                        var userInfo = user.Payload;
                        sender.Tell(new AuthenticationInformation(userInfo.DisplayName, userInfo.PasswordHash, userInfo.PasswordSalt));
                    }
                    else //failed fetch
                    {
                        sender.Tell(new MissingAuthenticationInformation(information.DisplayName));
                    }
                }
            });

            Receive<FetchUserByName>(async fetch =>
            {
                using (var repo = _userRepoFactory())
                {
                    var sender = Sender;
                    var user = await repo.FetchUser(fetch.DisplayName);
                    if (user.Status == ResultCode.Success && !(user.Payload is MissingUser)) //successful fetch
                    {
                        var userInfo = user.Payload;
                        sender.Tell(new UserIdentity(userInfo.UserId, userInfo.DisplayName, userInfo.Email, userInfo.DateJoined, userInfo.CookieId));
                    }
                    else //failed fetch
                    {
                        sender.Tell(new MissingUserIdentity(fetch.DisplayName));
                    }
                }
            });

            Receive<FetchUserByCookie>(async fetch =>
            {
                using (var repo = _userRepoFactory())
                {
                    var sender = Sender;
                    var user = await repo.FetchUser(fetch.CookieId);
                    if (user.Status == ResultCode.Success && !(user.Payload is MissingUser)) //successful fetch
                    {
                        var userInfo = user.Payload;
                        sender.Tell(new UserIdentity(userInfo.UserId, userInfo.DisplayName, userInfo.Email, userInfo.DateJoined, userInfo.CookieId));
                    }
                    else //failed fetch
                    {
                        sender.Tell(new MissingUserIdentity(string.Empty));
                    }
                }
            });
        }
    }
}