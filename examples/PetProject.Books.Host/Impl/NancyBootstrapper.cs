using Autofac;
using Nancy;
using Nancy.Bootstrappers.Autofac;
using Nancy.Json;

namespace PetProject.Books.Host.Impl
{
    public class NancyBootstrapper : AutofacNancyBootstrapper
    {
        private readonly ILifetimeScope ioc;

        public NancyBootstrapper(ILifetimeScope ioc)
        {
            this.ioc = ioc;
            JsonSettings.RetainCasing = true;
        }

        protected override ILifetimeScope GetApplicationContainer()
        {
            return ioc;
        }

#if DEBUG
        protected override IRootPathProvider RootPathProvider
        {
            get { return new NancyDebugRootPathProvider(); }
        }
#endif
    }

}
