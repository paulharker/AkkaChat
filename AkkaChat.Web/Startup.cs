using AkkaChat.Infrastructure;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Extensions;
using Nancy.Authentication.Forms;
using Nancy.Cryptography;
using Nancy.Owin;
using Ninject;
using Owin;

namespace AkkaChat
{
    /// <summary>
    /// OWIN Startup
    /// </summary>
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            RunDbSetup();
            var kernel = SetupNinject();
            StartActorSystem(kernel);
            BindActorDependencies(kernel);

            SetupSignalR(kernel, app);
            SetupNancy(kernel, app);
            StartSignalRDependentActors(kernel);
        }

        private static void SetupNancy(IKernel kernel, IAppBuilder app)
        {
            var cryptographyConfiguration = new CryptographyConfiguration(
                new RijndaelEncryptionProvider(new PassphraseKeyGenerator("SuperSecretPass", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })),
                new DefaultHmacProvider(new PassphraseKeyGenerator("UberSuperSecure", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })));

            var formsAuthConfiguration = new FormsAuthenticationConfiguration()
            {
                CryptographyConfiguration = cryptographyConfiguration,
                RedirectUrl = "/login",
                UserMapper = kernel.Get<IUserMapper>()
            };

            var bootstrapper = new AkkaChatBootstrapper(kernel, formsAuthConfiguration);
            app.UseNancy(new NancyOptions() {Bootstrapper = bootstrapper});
            app.UseStageMarker(PipelineStage.MapHandler);
        }

        private static void SetupSignalR(IKernel kernel, IAppBuilder app)
        {
            var config = new HubConfiguration()
            {
                EnableJavaScriptProxies = true
            };
           
            app.MapSignalR(config); //enable websocket magic
        }
    }
}