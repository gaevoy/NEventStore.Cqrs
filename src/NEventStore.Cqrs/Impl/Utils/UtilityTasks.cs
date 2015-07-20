using CommonDomain.Persistence;
using NEventStore.Cqrs.Impl.Utils.Tasks;
using NEventStore.Cqrs.Utils;

namespace NEventStore.Cqrs.Impl.Utils
{
    public class UtilityTasks : IUtilityTasks
    {
        private ILogger log;
        private IDependencyResolver ioc;
        private IStoreEvents storeEvents;
        private IPersistHelper persistHelper;
        private IRepository repository;
        private string rmConnectionString;

        public UtilityTasks(ILogger log,
                            IDependencyResolver ioc,
                            IStoreEvents storeEvents,
                            IPersistHelper persistHelper,
                            IRepository repository)
        {
            this.log = log;
            this.ioc = ioc;
            this.storeEvents = storeEvents;
            this.persistHelper = persistHelper;
            this.repository = repository;
        }
        
        public void RebuildSnapshots(int maxEventsThreshold = 500)
        {
            var task = new RebuildSnapshotTask(log, storeEvents, repository);
            task.Run(maxEventsThreshold);
        }

        public void CheckAggregatesReplay()
        {
            var task = new CheckEventStoreTask(log, repository, persistHelper);
            task.Run();
        }

        public void ClearSnapshots()
        {
            var task = new ClearSnapshotTask(log, persistHelper);
            task.Run();
        }
    }
}
