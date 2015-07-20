using System;
using System.Configuration;
using CommonDomain.Persistence;
using NEventStore.Cqrs.EventStream.Projector.NEventStore;
using NEventStore.Cqrs.Impl.Utils;
using NEventStore.Cqrs.Projections;
using NEventStore.Cqrs.Sqlite.Projections;
using NEventStore.Cqrs.Utils;
using NEventStore.Cqrs.Utils.History;

namespace NEventStore.Cqrs.Sqlite
{
    public class CqrsSqliteWireup : Wireup
    {
        protected CqrsSqliteWireup(NanoContainer container)
            : base(container)
        {

        }

        public CqrsSqliteWireup(CqrsWireup wireup, string connectionName, string readModelsConnectionName)
            : base(wireup)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            string readModelsConnectionString = ConfigurationManager.ConnectionStrings[readModelsConnectionName].ConnectionString;
            Register<IPersistHelper>(_ => new PersistHelper(connectionString))
                .Register<ICheckpointStore>(_ => new CheckpointStore(readModelsConnectionString))
                .Register<IVersioningRepository>(_ => new VersioningRepository(readModelsConnectionString))
                .Register<IHistoryReader>(ioc => new NEventStoreStream(ioc.Resolve<IStoreEvents>()))
                .Register<IUtilityTasks>(ioc => new UtilityTasks(ioc.Resolve<ILogger>(),
                    ioc.Resolve<IDependencyResolver>(), 
                    ioc.Resolve<IStoreEvents>(),
                    ioc.Resolve<IHistoryReader>(),
                    ioc.Resolve<IVersioningRepository>(),
                    ioc.Resolve<ICheckpointStore>(),
                    ioc.Resolve<IPersistHelper>(),
                    ioc.Resolve<IRepository>(),
                    readModelsConnectionString));
        }

        CqrsSqliteWireup Register<T>(Func<NanoContainer, T> func) where T : class
        {
            Container.Register(func);
            return this;
        }
    }
}
