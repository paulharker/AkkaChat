using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using AkkaChat.Data;
using AkkaChat.Data.Repositories;
using AkkaChat.Data.Repositories.SQL;
using AkkaChat.Hubs;
using AkkaChat.Infrastructure;
using AkkaChat.Services;
using Microsoft.AspNet.SignalR;
using MVC.Utilities.Encryption;
using Nancy.Authentication.Forms;
using Nancy.Bootstrappers.Ninject;
using Ninject;
using Ninject.Modules;

namespace AkkaChat
{
    public partial class Startup
    {
        private static IKernel SetupNinject()
        {
            var kernel = new StandardKernel(new INinjectModule[] {new FactoryModule()});

            //SQL Schema information
            var connectionString = ConfigurationManager.ConnectionStrings["akkaChat"];
            var schemaName = ConfigurationManager.AppSettings["SqlSchemaName"];
            var chatTableName = ConfigurationManager.AppSettings["ChatTableName"];
            var userTableName = ConfigurationManager.AppSettings["UsersTableName"];

            //Reuse the same instance
            kernel.Bind<SqlQueries>()
                .ToMethod(context1 => new SqlQueries(schemaName, userTableName, chatTableName))
                .InSingletonScope();

            kernel.Bind<IDbConnection>()
                .To<SqlConnection>()
                .WithConstructorArgument("connectionString", connectionString.ConnectionString);

            kernel.Bind<IUserRepository>().To<SqlUserRepository>();
            kernel.Bind<IRoomRepository>().To<SqlRoomRepository>();

            kernel.Bind<Func<IUserRepository>>().ToMethod(context1 => () => context1.Kernel.Get<IUserRepository>());
            kernel.Bind<Func<IRoomRepository>>().ToMethod(context1 => () => context1.Kernel.Get<IRoomRepository>());

            //Cryptography information
            kernel.Bind<ICryptoService>()
                .To<HMACSHA1Service>()
                .WithConstructorArgument("validationKey", ConfigurationManager.AppSettings["ValidationKey"]);

            return kernel;
        }

        private static void BindActorDependencies(IKernel container)
        {
            /*
             * Membership system (powered by... actors!)
             */
            container.Bind<IMembershipService>().To<ActorMembershipService>()
                .WithConstructorArgument("identityManager", ActorSystemRefs.Identity)
                .WithConstructorArgument("sessionManager", ActorSystemRefs.SessionManager);

            container.Bind<IUserMapper>()
                .To<AkkaChatUserMapper>()
                .WithConstructorArgument("sessionManager", ActorSystemRefs.SessionManager);

            container.Bind<IUserIdProvider>().To<AkkaChatUserMapper>()
                .WithConstructorArgument("sessionManager", ActorSystemRefs.SessionManager);


        }
    }
}