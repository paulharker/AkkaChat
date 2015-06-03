using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Nancy.Cryptography;
using Ninject;

namespace AkkaChat
{
    using Nancy;

    public class AkkaChatBootstrapper : NinjectNancyBootstrapper
    {
        private readonly IKernel _kernel;
        private readonly FormsAuthenticationConfiguration _formsAuthenticationConfiguration;

        public AkkaChatBootstrapper(IKernel kernel, FormsAuthenticationConfiguration formsAuthenticationConfiguration)
        {
            _kernel = kernel;
            _formsAuthenticationConfiguration = formsAuthenticationConfiguration;
        }

        protected override IKernel GetApplicationContainer()
        {
            return _kernel;
        }

        protected override void ApplicationStartup(IKernel container, IPipelines pipelines)
        {
            FormsAuthentication.Enable(pipelines, _formsAuthenticationConfiguration);
        }
    }
}