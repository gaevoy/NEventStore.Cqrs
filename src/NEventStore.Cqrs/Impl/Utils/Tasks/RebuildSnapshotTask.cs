using System;
using System.Linq;
using CommonDomain;
using CommonDomain.Persistence;
using NEventStore.Persistence;

namespace NEventStore.Cqrs.Impl.Utils.Tasks
{
    public class RebuildSnapshotTask
    {
        private readonly ILogger log;
        private readonly IStoreEvents events;
        private readonly IRepository repository;

        public RebuildSnapshotTask(ILogger log, IStoreEvents events, IRepository repository)
        {
            if (log == null) throw new ArgumentNullException("log");
            if (events == null) throw new ArgumentNullException("events");
            if (repository == null) throw new ArgumentNullException("repository");
            this.log = log;
            this.events = events;
            this.repository = repository;
        }

        public void Run(int maxEventsThreshold = 500)
        {
            log.Info("Rebuild snapshots. MaxEventsThreshold: " + maxEventsThreshold);
            IPersistStreams advanced = events.Advanced;
            var ids = advanced.GetStreamsToSnapshot(maxEventsThreshold).Select(e => new Guid(e.StreamId)).ToArray();
            ids.AsParallel().ForAll(MakeSnapshot);
        }

        private void MakeSnapshot(Guid streamId)
        {
            try
            {
                var aggr = repository.GetById<IAggregate>(streamId);
                var snapshot = aggr.GetSnapshot();
                events.Advanced.AddSnapshot(new Snapshot(snapshot.Id.ToString(), snapshot.Version, snapshot));
                log.Info(string.Format("Snapshot added for {0} no of events {1}", aggr.GetType().Name, snapshot.Version));
            }
            catch { }
        }
    }
}