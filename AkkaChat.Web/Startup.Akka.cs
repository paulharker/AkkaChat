using System;
using Akka.Actor;
using Akka.DI.Ninject;
using Akka.Persistence;
using Akka.Persistence.SqlServer;
using Akka.Routing;
using AkkaChat.Actors;
using Ninject;

namespace AkkaChat
{
    public partial class Startup
    {
        private static void StartActorSystem(IKernel container)
        {
            ActorSystemRefs.Kernel = container;

            /*
            * Initialize ActorSystem and essential system actors
            */
            var sys = ActorSystemRefs.System = ActorSystem.Create("AkkaChat");

            /*
           * Enable SQL Server Akka.Persistence
           */
            var resolver = ActorSystemRefs.DiResolver = new NinjectDependencyResolver(ActorSystemRefs.Kernel, ActorSystemRefs.System);
            var identity = ActorSystemRefs.Identity = sys.ActorOf(resolver.Create<IdentityManager>(), "identity");
            container.Bind<AuthenticationChallengeActor>().ToSelf()
                .WithConstructorArgument("authTimeout", TimeSpan.FromSeconds(3.0))
                .WithConstructorArgument("userIdentityStore", identity);
            var authChallenge = ActorSystemRefs.Authenticator = sys.ActorOf(resolver.Create<AuthenticationChallengeActor>(), "authenticator");
            var sessionManager =
                ActorSystemRefs.SessionManager =
                    sys.ActorOf(Props.Create(() => new UserSessionManager(identity, authChallenge)), "sessions");

            

        }

        private static void StartSignalRDependentActors(IKernel container)
        {
            var sys = ActorSystemRefs.System;

            var signalrWriter =
                ActorSystemRefs.SignalRWriter =
                    sys.ActorOf(Props.Create(() => new SignalrWriter()).WithRouter(FromConfig.Instance),
                        "signalr-writer");

            var roomMaster =
               ActorSystemRefs.RoomMaster = sys.ActorOf(Props.Create(() => new RoomMaster(signalrWriter)), "rooms");

            var signalrReader =
                ActorSystemRefs.SignalRReader =
                    sys.ActorOf(Props.Create(() => new SignalrReader(roomMaster, ActorSystemRefs.SessionManager)).WithRouter(FromConfig.Instance),
                        "signalr-reader");
        }
    }
}