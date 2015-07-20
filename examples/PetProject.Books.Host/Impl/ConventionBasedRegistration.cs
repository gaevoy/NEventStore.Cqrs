using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Autofac;
using EventStream.Projector;
using EventStream.Projector.Impl;
using EventStream.Projector.Persistence;
using NEventStore.Cqrs.MongoDb.EventStream.Projector.MongoDb;

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


            builder.Register<ICheckpointStore>(_ => new CheckpointStore(rmConnectionString));
            builder.Register<IProjectionInfoStore>(_ => new ProjectionInfoStore(rmConnectionString));
            builder.Register<IProjector>(ioc => new SimpleProjector(ioc.Resolve<IEnumerable<IProjection>>().ToArray(), ioc.Resolve<ICheckpointStore>()));
            builder.Register<IProjectionRebuild>(ioc => new ProjectionRebuild(ioc.Resolve<IEnumerable<IProjection>>().ToArray(), ioc.Resolve<ICheckpointStore>(), ioc.Resolve<IProjectionInfoStore>()));
        }
    }
}
