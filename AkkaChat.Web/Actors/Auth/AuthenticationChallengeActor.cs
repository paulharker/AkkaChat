using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using AkkaChat.Messages;
using MVC.Utilities.Encryption;

namespace AkkaChat.Actors
{
    /// <summary>
    /// Actor responsible for handling authentication challenges
    /// </summary>
    public class AuthenticationChallengeActor : ReceiveActor
    {
        #region Internal message classes

        public sealed class PendingAuthentication : IEquatable<PendingAuthentication>
        {
            public PendingAuthentication(AuthenticationChallenge challenge, IActorRef requestor, ICancelable timeout)
            {
                Requestor = requestor;
                Challenge = challenge;
                Timeout = timeout;
            }

            public AuthenticationChallenge Challenge { get; set; }

            public IActorRef Requestor { get; set; }

            public ICancelable Timeout { get; set; }

            public bool Equals(PendingAuthentication other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(Challenge, other.Challenge) && Equals(Requestor, other.Requestor);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((PendingAuthentication) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Challenge != null ? Challenge.GetHashCode() : 0)*397) ^ (Requestor != null ? Requestor.GetHashCode() : 0);
                }
            }
        }

        public sealed class CheckAuthenticationTick
        {
            public CheckAuthenticationTick(AuthenticationChallenge auth)
            {
                Auth = auth;
            }

            public AuthenticationChallenge Auth { get; set; }
        }

        #endregion

        private readonly HashSet<PendingAuthentication> _pendingAuthentications;
        private readonly ICryptoService _crypto;
        private readonly IActorRef _userIdentityStore;
        private readonly TimeSpan _authTimeout;
        private readonly ILoggingAdapter _log = Context.GetLogger();

        public AuthenticationChallengeActor(ICryptoService crypto, TimeSpan authTimeout, IActorRef userIdentityStore)
        {
            _crypto = crypto;
            _authTimeout = authTimeout;
            _userIdentityStore = userIdentityStore;
            _pendingAuthentications = new HashSet<PendingAuthentication>();

            Receive<AuthenticationChallenge>(challenge =>
            {
                //ignore any duplicates
                if (!_pendingAuthentications.Any(x => x.Challenge.Equals(challenge) && x.Requestor.Equals(Sender)))
                {
                    //schedule an auth timeout to ourselves
                    var timeout = Context.System.Scheduler.ScheduleTellOnceCancelable(_authTimeout, Self, new CheckAuthenticationTick(challenge), Self);

                    //request authentication information from the IdentityStore
                    _userIdentityStore.Tell(new IdentityStoreActor.FetchAuthenticationInformation(challenge.DisplayName));

                    _pendingAuthentications.Add(new PendingAuthentication(challenge, Sender, timeout));
                }
            });

            Receive<MissingAuthenticationInformation>(missingAuthInformation =>
            {
                /* Challenge failed on the grounds of MISSING USER */
                var auth =
                    _pendingAuthentications.FirstOrDefault(
                        x => x.Challenge.DisplayName.Equals(missingAuthInformation.DisplayName));
                if (auth == null)
                {
                    _log.Warning(
                        "Couldn't find authentication information for user {0}, but also had no record of them in pending authentications list.",
                        missingAuthInformation.DisplayName);
                }
                else
                {
                    _pendingAuthentications.Remove(auth);
                    auth.Timeout.Cancel();
                    auth.Requestor.Tell(new AuthenticationFailure(missingAuthInformation.DisplayName, 
                        string.Format("Account name {0} not recognized.", missingAuthInformation.DisplayName)));
                }
            });

            Receive<AuthenticationInformation>(info =>
            {
                /* FOUND matching authentication information */
                var auth =
                   _pendingAuthentications.FirstOrDefault(
                       x => x.Challenge.DisplayName.Equals(info.DisplayName));
                if (auth == null)
                {
                    _log.Warning(
                        "Found authentication information for user {0}, but have no record of them in pending authentications list.",
                        info.DisplayName);
                }
                else
                {
                    var success = _crypto.CheckPassword(auth.Challenge.AttemptedPassword, info.HashedPassword, info.Salt);
                    if (success) /* AUTHENTICATION SUCCESS */
                    {
                        _userIdentityStore.Ask<UserIdentity>(new IdentityStoreActor.FetchUserByName(info.DisplayName))
                            .ContinueWith<object>(tr =>
                            {
                                if(tr.IsFaulted || tr.Result is MissingUserIdentity)
                                    return new AuthenticationFailure(info.DisplayName, "Authentication attempt was successful, but connection to database failed during final handshake. Please retry!");
                                return new AuthenticationSuccess(tr.Result, tr.Result.CookieId); //create a new SessionID
                            }).PipeTo(auth.Requestor);
                    }
                    else //AUTHENTICATION FAILURE - PASSWORD MISMATCH
                    {
                        auth.Requestor.Tell(new AuthenticationFailure(info.DisplayName, "Login failed. Wrong password."));
                    }

                    _pendingAuthentications.Remove(auth);
                    auth.Timeout.Cancel();
                }
            });

            Receive<CheckAuthenticationTick>(tick =>
            {
                /* Challenge failed on grounds of TIMEOUT */
                var auth =
                  _pendingAuthentications.FirstOrDefault(
                      x => x.Challenge.DisplayName.Equals(tick.Auth.DisplayName));
                if (auth == null)
                {
                    _log.Warning(
                        "Auth timeout for user {0}, but have no record of them in pending authentications list.",
                        tick.Auth.DisplayName);
                }
                else
                {
                    auth.Requestor.Tell(new AuthenticationFailure(tick.Auth.DisplayName, "Authentication attempt timed out."));
                    _pendingAuthentications.Remove(auth);
                    auth.Timeout.Cancel();
                }
            });
        }

        protected override void PostStop()
        {
            /* cancel all scheduled message deliveries */
            foreach (var pendingAuthentication in _pendingAuthentications)
            {
                pendingAuthentication.Timeout.Cancel();

                //fail all pending authentication challenges
                pendingAuthentication.Requestor.Tell(new AuthenticationFailure(pendingAuthentication.Challenge.DisplayName, "Auth system restart. Please try again!"));
            }
            base.PostStop();
        }
    }
}