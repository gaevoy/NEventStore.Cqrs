using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using CommonDomain.Persistence;
using MongoDB.Bson.Serialization;
using NEventStore.Cqrs.Impl.Utils;
using NEventStore.Cqrs.Impl.Utils.History;
using NEventStore.Cqrs.Messages;
using NEventStore.Cqrs.MongoDb.Projections;
using NEventStore.Cqrs.Projections;
using NEventStore.Cqrs.Utils;
using NEventStore.Cqrs.Utils.History;

namespace NEventStore.Cqrs.MongoDb
{
    public class CqrsMongoWireup : Wireup
    {
        protected CqrsMongoWireup(NanoContainer container)
            : base(container)
        {

        }

        public CqrsMongoWireup(CqrsWireup wireup, string connectionName, string readModelsConnectionName, params Assembly[] assemblies)
            : base(wireup)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            string readModelsConnectionString = ConfigurationManager.ConnectionStrings[readModelsConnectionName].ConnectionString;
            Register<IPersistHelper>(_ => new PersistHelper(connectionString))
                .Register<ICheckpointStore>(_ => new CheckpointStore(readModelsConnectionString))
                .Register<IVersioningRepository>(_ => new VersioningRepository(readModelsConnectionString))
                .Register<IHistoryReader>(ioc => new EventStoreHistoryReader(ioc.Resolve<IStoreEvents>()))
                .Register<IUtilityTasks>(ioc => new UtilityTasks(ioc.Resolve<ILogger>(),
                    ioc.Resolve<IDependencyResolver>(),
                    ioc.Resolve<IStoreEvents>(),
                    ioc.Resolve<IHistoryReader>(),
                    ioc.Resolve<IVersioningRepository>(),
                    ioc.Resolve<ICheckpointStore>(),
                    ioc.Resolve<IPersistHelper>(),
                    ioc.Resolve<IRepository>(),
                    readModelsConnectionString));

            var evt = typeof(IEvent);
            var cmd = typeof(ICommand);
            var types = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && (evt.IsAssignableFrom(t) || cmd.IsAssignableFrom(t)));

            foreach (var t in types)
                BsonClassMap.LookupClassMap(t);
        }

        CqrsMongoWireup Register<T>(Func<NanoContainer, T> func) where T : class
        {
            Container.Register(func);
            return this;
        }
    }
}
