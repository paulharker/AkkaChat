using System;
using Akka.Actor;
using Akka.Event;
using AkkaChat.Infrastructure;
using AkkaChat.Messages;

namespace AkkaChat.Actors
{
    /// <summary>
    /// Actor session that corresponds 1:1 with an active user
    /// </summary>
    public class UserSessionActor : ReceiveActor, IWithUnboundedStash
    {
        #region Built-in messages

        public class FetchIdentity
        {
            public FetchIdentity(string displayName)
            {
                DisplayName = displayName;
            }

            public string DisplayName { get; private set; }
        }

        public class CheckSession
        {
            public CheckSession(Guid cookieId)
            {
                CookieId = cookieId;
            }

            public Guid CookieId { get; private set; }
        }

        public class UnknownSession
        {
            public UnknownSession(Guid cookieId)
            {
                CookieId = cookieId;
            }

            public Guid CookieId { get; private set; }
        }

        public class ValidSession
        {
            public ValidSession(AkkaChatIdentity identity)
            {
                Identity = identity;
            }

            public AkkaChatIdentity Identity { get; private set; }
        }

        public class InvalidateSession { }

        #endregion

        private readonly Guid _cookieId;
        private UserIdentity _userIdentity;

        protected UserIdentity Identity
        {
            get { return _userIdentity; }
            set
            {
                _userIdentity = value;
                _akkaChatIdentity = new AkkaChatIdentity(_userIdentity);
                _sessionAck = new ValidSession(_akkaChatIdentity);
            }
        }

        private ValidSession _sessionAck;
        private AkkaChatIdentity _akkaChatIdentity;
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly IActorRef _identityActor;

        public IStash Stash { get; set; }

        public UserSessionActor(Guid cookieId, IActorRef identityActor)
        {
            _cookieId = cookieId;
            _identityActor = identityActor;
            WaitingForIdentity();
        }

        private void WaitingForIdentity()
        {
            Receive<MissingUserIdentity>(missing => PublishAndThrow(_cookieId));

            Receive<UserIdentity>(identity =>
            {
                Identity = identity;
                Context.Parent.Tell(identity);
                BecomeReady();
            });

            //Timed out - didn't hear back from identity actor
            Receive<ReceiveTimeout>(timeout => PublishAndThrow(_cookieId));

            //stash any other messages
            ReceiveAny(o => Stash.Stash());
        }

        private void BecomeReady()
        {
            SetReceiveTimeout(null); //cancels the ReceiveTimeout
            Become(Ready);
            Stash.UnstashAll();
        }

        private void Ready()
        {
            //Check to see if a particular session is valid
            Receive<CheckSession>(session =>
            {
                if (session.CookieId == _cookieId)
                {
                    Sender.Tell(_sessionAck);
                }
                else
                {
                    Sender.Tell(new UnknownSession(session.CookieId));
                }
            });

            Receive<FetchIdentity>(identity =>
            {
                if (identity.DisplayName.Equals(Identity.DisplayName))
                {
                    Sender.Tell(Identity);
                }
                else
                {
                    Sender.Tell(new MissingUserIdentity(identity.DisplayName));
                }
            });
        }

        private void PublishAndThrow(Guid cookieId)
        {
            _log.Error("Not able to locate user for cookie ID {0}", cookieId);
            throw new UnidentifiedAkkaSessionException(string.Format("Could not locate user with cookieID {0}", cookieId), cookieId);
        }

        protected override void PreStart()
        {
           _identityActor.Tell(new IdentityStoreActor.FetchUserByCookie(_cookieId));
            SetReceiveTimeout(TimeSpan.FromSeconds(1.5));
        }
    }
}