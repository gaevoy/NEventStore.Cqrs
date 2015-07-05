using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EventStream.Projector.Impl
{
    public class Projector : IProjector
    {
        private readonly ProjectionWrapper[] projections;
        private readonly ILog log;
        private readonly ICheckpoint checkpoint;
        private readonly CheckpointScope checkpointScope;

        public Projector(IProjection[] projections, ILog log, ICheckpoint checkpoint, CheckpointScope checkpointScope = CheckpointScope.Regular)
        {
            this.projections = projections.Select(e => new ProjectionWrapper(e, log)).ToArray();
            this.log = log;
            this.checkpoint = checkpoint;
            this.checkpointScope = checkpointScope;
        }

        public void Handle(EventsSlice evts)
        {
            foreach (var evt in evts.Events)
                foreach (var projection in projections)
                    projection.Handle(evt);
            checkpoint.Save(evts.Position, checkpointScope);
        }

        public void HandleAll(IEnumerable<EventsSlice> evts)
        {
            EventsSlice processed = null;
            foreach (var projection in projections)
                projection.Begin();
            foreach (var evtSlice in ShowLogs(evts))
            {
                foreach (var evt in evtSlice.Events)
                    foreach (var projection in projections)
                        projection.Handle(evt);
                processed = evtSlice;
            }
            foreach (var projection in projections)
                projection.Flush();
            if (processed != null)
                checkpoint.Save(processed.Position, checkpointScope);
        }

        private IEnumerable<EventsSlice> ShowLogs(IEnumerable<EventsSlice> commits)
        {
            EventsSlice processed = null;
            long noOfCommits = 0;
            foreach (var commit in commits)
            {
                noOfCommits++;
                yield return commit;

                processed = commit;
                if (noOfCommits % 10000 == 0)
                    log.Debug("Projection statistics" + Environment.NewLine + ProjectionWrapper.GetStatistic(projections, take: 3));
            }
            if (processed != null)
            {
                log.Info("Projection statistics" + Environment.NewLine + ProjectionWrapper.GetStatistic(projections, take: 20, takeEvents: 5));
            }
        }
    }
}
