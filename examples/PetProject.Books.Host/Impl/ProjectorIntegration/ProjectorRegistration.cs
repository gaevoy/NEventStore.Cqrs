using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Autofac;
using EventStream.Projector;
using EventStream.Projector.Impl;
using EventStream.Projector.Logger;
using EventStream.Projector.Persistence;
using NEventStore.Cqrs.MongoDb.EventStream.Projector.MongoDb;

namespace PetProject.Books.Host.Impl.ProjectorIntegration
{
    public class ProjectorRegistration : Autofac.Module
    {
        private readonly string readConnectionString;

        public ProjectorRegistration(string readConnectionString)
        {
            this.readConnectionString = readConnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            string rmConnectionString = ConfigurationManager.ConnectionStrings[readConnectionString].ConnectionString;
            builder.RegisterInstance(new Log4NetLog()).AsImplementedInterfaces();
            builder.Register<ICheckpointStore>(_ => new CheckpointStore(rmConnectionString));
            builder.Register<IProjectionInfoStore>(_ => new ProjectionInfoStore(rmConnectionString));
            builder.Register<IProjector>(ioc => new SimpleProjector(
                ioc.Resolve<IEnumerable<IProjection>>().ToArray(),
                ioc.Resolve<ICheckpointStore>(),
                ioc.Resolve<ILog>()));
            builder.Register<IProjectionRebuild>(ioc => new ProjectionRebuild(
                ioc.Resolve<IEnumerable<IProjection>>().ToArray(),
                ioc.Resolve<ICheckpointStore>(),
                ioc.Resolve<IProjectionInfoStore>(),
                ioc.Resolve<ILog>()));
        }
    }
}
