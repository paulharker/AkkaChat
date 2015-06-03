using Akka.Actor;
using Akka.DI.Core;
using Ninject;

namespace AkkaChat
{
    /// <summary>
    /// Static class used to access the <see cref="ActorSystem"/> throughout Nancy and SignalR controllers
    /// </summary>
    public static class ActorSystemRefs
    {
        /// <summary>
        /// Reference to the <see cref="ActorSystem"/>
        /// </summary>
        public static ActorSystem System;

        /// <summary>
        /// Dependency injection resolver for Akka.NET actors
        /// </summary>
        public static IDependencyResolver DiResolver;

        /// <summary>
        /// Hang onto a reference to the Ninject kernel directly
        /// </summary>
        public static IKernel Kernel { get; set; }

        /// <summary>
        /// All identity-based actors
        /// </summary>
        public static IActorRef Identity { get; set; }

        /// <summary>
        /// Authentication actor
        /// </summary>
        public static IActorRef Authenticator { get; set; }

        /// <summary>
        /// Session manager
        /// </summary>
        public static IActorRef SessionManager { get; set; }

        /// <summary>
        /// SignalR writer
        /// </summary>
        public static IActorRef SignalRWriter { get; set; }

        /// <summary>
        /// SignalR reader
        /// </summary>
        public static IActorRef SignalRReader { get; set; }

        /// <summary>
        /// Ultimate authority on which chat rooms are available
        /// </summary>
        public static IActorRef RoomMaster { get; set; }
    }
}