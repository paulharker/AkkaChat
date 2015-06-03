using System;
using Akka.Actor;
using Akka.DI.Core;

namespace AkkaChat.Actors
{
    /// <summary>
    /// PARENT PROXY PATTERN.
    /// 
    /// Creates <see cref="IdentityWriterActor"/> instances upon request but doesn't
    /// really engage with them. Really serves as a broker between actors who need a user
    /// created and the <see cref="IdentityWriterActor"/>.
    /// </summary>
    public class IdentityManager : ReceiveActor
    {
        #region Message types

        /// <summary>
        /// Request an actor reference to the identity store
        /// </summary>
        public class GetIdentityStore { }

        #endregion

        private IActorRef _identityStore;

        public IdentityManager()
        {
            Receive<IdentityWriterActor.CreateUser>(create =>
            {
                var writer = Context.DI().ActorOf<IdentityWriterActor>();
                writer.Forward(create); //forward the current sender to the writer
            });

            //do nothing
            Receive<Terminated>(terminated => { });

            //pass the identity of the store directly to the caller
            Receive<GetIdentityStore>(store => Sender.Tell(_identityStore));

            //forward all other types of identity-related messages to the IdentityStoreActor
            ReceiveAny(m => _identityStore.Forward(m));
        }

        protected override void PreStart()
        {
            _identityStore = Context.DI().ActorOf<IdentityStoreActor>("identity");
        }

        protected override void PreRestart(Exception reason, object message)
        {
            //don't kill off our children
        }
    }
}