using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaChat.Infrastructure;
using AkkaChat.Messages;

namespace AkkaChat.Actors
{
    /// <summary>
    /// Responsible for managing all user sessions
    /// </summary>
    public class UserSessionManager : ReceiveActor
    {
        #region Message classes
        #endregion

        #region SupervisorStrategy

        /// <summary>
        /// STOP any children who throw an <see cref="UnidentifiedAkkaSessionException"/> message.
        /// </summary>
        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(Decider.From(Directive.Restart, 
                new KeyValuePair<Type, Directive>(typeof(UnidentifiedAkkaSessionException), Directive.Stop)));
        }

        #endregion

        private readonly IActorRef _identityStore;
        private readonly IActorRef _authChallenge;
        private Dictionary<string, IActorRef> _usersByName;

        public UserSessionManager(IActorRef identityStore, IActorRef authChallenge)
        {
            _identityStore = identityStore;
            _authChallenge = authChallenge;
            _usersByName = new Dictionary<string, IActorRef>();
            Ready();
        }

        private void Ready()
        {
            Receive<UserSessionActor.CheckSession>(session =>
            {
                var userSessionActor = LookupOrCreateUserSessionActor(session.CookieId);
                userSessionActor.Forward(session);
            });

            Receive<AuthenticationChallenge>(challenge =>
            {
                var sender = Sender;
                var context = Context; //need to capture context here too
                _authChallenge.Ask<object>(challenge, TimeSpan.FromSeconds(5.0))
                    .ContinueWith(tr =>
                    {
                        if (tr.IsFaulted) //99% of the time, this will be a timeout
                        {
                            sender.Tell(new AuthenticationFailure(challenge.DisplayName, "Authentication timed out."));
                        }
                        else
                        {
                            var authResult = tr.Result as AuthenticationSuccess;
                            if (authResult != null)
                            {
                                sender.Tell(authResult, context.Self); //let the caller know that we were successful
                                context.Self.Tell(authResult.User); //send ourselves a copy of the identity
                                LookupOrCreateUserSessionActor(context, authResult.CookieId); //create the session actor
                            }
                            else
                            {
                                sender.Tell(tr.Result, context.Self);
                            }
                        }
                    }, TaskContinuationOptions.ExecuteSynchronously);
            });

            Receive<UserIdentity>(identity =>
            {
                _usersByName[identity.DisplayName] = LookupOrCreateUserSessionActor(identity.CookieId);
            });

            Receive<UserSessionActor.FetchIdentity>(identity =>
            {
                if (_usersByName.ContainsKey(identity.DisplayName))
                {
                    _usersByName[identity.DisplayName].Forward(identity);
                }
                else
                {
                    Sender.Tell(new MissingUserIdentity(identity.DisplayName));
                }
            });
        }

        private IActorRef LookupOrCreateUserSessionActor(Guid cookieId)
        {
            return LookupOrCreateUserSessionActor(Context, cookieId);
        }

        private IActorRef LookupOrCreateUserSessionActor(IActorContext context, Guid cookieId)
        {
            var child = context.Child(cookieId.ToString());
            if (child.Equals(ActorRefs.Nobody)) //child doesn't exist
            {
                return Context.ActorOf(Props.Create(() => new UserSessionActor(cookieId, _identityStore)),
                    cookieId.ToString());
            }
            return child;
        }

        protected override void PreRestart(Exception reason, object message)
        {
            //preserve all children in the event of a restart
        }
    }
}