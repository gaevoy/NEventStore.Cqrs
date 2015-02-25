using System.Configuration;
using System.Reflection;
using Autofac;

namespace PetProject.Books.Host.Impl
{
    public class ConventionBasedRegistration : Autofac.Module
    {
        private readonly Assembly[] appAssemblies;
        private readonly string readConnectionString;

        public ConventionBasedRegistration(Assembly[] appAssemblies, string readConnectionString)
        {
            this.appAssemblies = appAssemblies;
            this.readConnectionString = readConnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            string rmConnectionString = ConfigurationManager.ConnectionStrings[readConnectionString].ConnectionString;
            builder.RegisterAssemblyTypes(appAssemblies)
                .Where(t => t.Name.EndsWith("CommandHandlers") || t.Name.EndsWith("CommandHandler") || t.Name.EndsWith("EventHandler") || t.Name.EndsWith("EventHandlers"))
                .AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(appAssemblies)
                .Where(t => t.Name.EndsWith("Projection"))
                .AsImplementedInterfaces()
                .WithParameter(new PositionalParameter(0, rmConnectionString))
                .SingleInstance();
        }
    }
}
