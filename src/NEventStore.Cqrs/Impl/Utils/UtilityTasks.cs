using System.Threading;
using CommonDomain.Persistence;
using NEventStore.Cqrs.Impl.Utils.Tasks;
using NEventStore.Cqrs.Projections;
using NEventStore.Cqrs.Utils;
using NEventStore.Cqrs.Utils.History;

namespace NEventStore.Cqrs.Impl.Utils
{
    public class UtilityTasks : IUtilityTasks
    {
        private ILogger log;
        private IDependencyResolver ioc;
        private IStoreEvents storeEvents;
        private readonly IHistoryReader historyReader;
        private IVersioningRepository versioningRepository;
        private ICheckpointStore checkpoints;
        private IPersistHelper persistHelper;
        private IRepository repository;
        private string rmConnectionString;

        public UtilityTasks(ILogger log,
                            IDependencyResolver ioc,
                            IStoreEvents storeEvents,
                            IHistoryReader historyReader,
                            IVersioningRepository versioningRepository,
                            ICheckpointStore checkpoints,
                            IPersistHelper persistHelper,
                            IRepository repository,
                            string rmConnectionString)
        {
            this.log = log;
            this.ioc = ioc;
            this.storeEvents = storeEvents;
            this.historyReader = historyReader;
            this.versioningRepository = versioningRepository;
            this.checkpoints = checkpoints;
            this.rmConnectionString = rmConnectionString;
            this.persistHelper = persistHelper;
            this.repository = repository;
        }

        public void RebuildProjections(CancellationToken? pause = null, bool fromTheStart = false)
        {
            var rebuild = new RebuildTask(log, ioc, historyReader, versioningRepository, checkpoints, pause);
            if (fromTheStart) rebuild.ResetCheckpoints();
            rebuild.Run();
        }

        public void RebuildSnapshots(int maxEventsThreshold = 500)
        {
            var task = new RebuildSnapshotTask(log, storeEvents, repository);
            task.Run(maxEventsThreshold);
        }

        public void PrintChangedProjections()
        {
            var task = new ShowChangedProjectionsTask(log, ioc, versioningRepository, rmConnectionString);
            task.Run();
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
