using System;
using Akka.Actor;
using AkkaChat.Data.Models;
using AkkaChat.Data.Repositories;
using AkkaChat.Messages;

namespace AkkaChat.Actors
{
    /// <summary>
    /// Actor responsible for creating new users
    /// </summary>
    public class IdentityWriterActor : ReceiveActor
    {
        #region Messages

        /// <summary>
        /// Command for creating new <see cref="User"/> instances
        /// </summary>
        public class CreateUser
        {
            public CreateUser(string displayName, string email, string passwordHash, string passwordSalt, DateTime dateJoined, Guid cookeId)
            {
                CookeId = cookeId;
                DateJoined = dateJoined;
                PasswordSalt = passwordSalt;
                PasswordHash = passwordHash;
                Email = email;
                DisplayName = displayName;
            }

            public string DisplayName { get; private set; }

            public string Email { get; private set; }

            public string PasswordHash { get; private set; }

            public string PasswordSalt { get; private set; }

            public DateTime DateJoined { get; private set; }

            public Guid CookeId { get; private set; }
        }

        public class UserCreateResult
        {
            public UserCreateResult(string displayName, bool success)
            {
                Success = success;
                DisplayName = displayName;
            }

            public string DisplayName { get; private set; }

            public bool Success { get; private set; }
        }

        #endregion

        private IUserRepository _repository;

        public IdentityWriterActor(IUserRepository repository)
        {
            _repository = repository;

            Receive<CreateUser>(user =>
            {
                var sender = Sender;
                repository.UserNameIsAvailable(user.DisplayName)
                    .ContinueWith(tr =>
                    {
                        return new CanProceed<CreateUser>(user, sender, tr.Result.Payload);
                    })
                    .PipeTo(Self);

                // time this operation out if we don't hear back within three seconds
                SetReceiveTimeout(TimeSpan.FromSeconds(3));
            });

            //shut down in the event of a timeout
            Receive<ReceiveTimeout>(timeout => Context.Stop(Self));

            Receive<CanProceed<CreateUser>>(proceed =>
            {
                SetReceiveTimeout(null); //disable receivetimeout
                if (proceed.CanExecute)
                {
                    var createUser = proceed.OriginalMessage;

                    // create the user
                    // TODO: could use AutoMapper here instead of manually doing the mapping.
                    var result = repository.CreateUser(new User()
                    {
                        DateJoined = createUser.DateJoined,
                        DisplayName = createUser.DisplayName,
                        Email = createUser.Email,
                        PasswordHash = createUser.PasswordHash,
                        PasswordSalt = createUser.PasswordSalt,
                        CookieId = createUser.CookeId
                    });

                    if (result.Status == ResultCode.Success)
                    {
                        //notify the original sender that we successfully created the user
                        proceed.IntendedDestination.Tell(new UserCreateResult(createUser.DisplayName,true));
                        Context.Stop(Self); //shut ourselves down
                        return; //done working
                    }                       
                }

                //let the caller know that we failed
                proceed.IntendedDestination.Tell(new UserCreateResult(proceed.OriginalMessage.DisplayName, false));
            });
        }

        protected override void PostStop()
        {
            //cleanup any repositories or disposable resources we might have used
            try
            {
                _repository.Dispose();
            }
            catch { }

            base.PostStop();
        }
    }
}