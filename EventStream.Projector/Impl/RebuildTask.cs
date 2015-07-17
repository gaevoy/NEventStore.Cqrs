using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace EventStream.Projector.Impl
{
    class RebuildTask : IRebuildTask
    {
        private readonly IProjection[] projections;
        private readonly IProjectionInfoStore versions;
        private readonly IEventStream eventStream;
        private readonly ILog log;
        private readonly ICheckpointStore checkpoints;
        private readonly CancellationToken running;

        public RebuildTask(IProjection[] projections, IProjectionInfoStore versions, IEventStream eventStream, ILog log, ICheckpointStore checkpoints, CancellationToken running)
        {
            this.projections = projections;
            this.versions = versions;
            this.eventStream = eventStream;
            this.log = log;
            this.checkpoints = checkpoints;
            this.running = running;
        }

        public void Start()
        {
            if (running.IsCancellationRequested) return;

            var projectionСhangeCheckpoint = checkpoints.Restore(CheckpointScope.ProjectionChange);
            var regularCheckpoint = checkpoints.Restore(CheckpointScope.Default);

            // If regular checkpoint is undefined rebuild all projections
            if (regularCheckpoint == null)
            {
                MarkAsUnmodified(projections);
            }
            else
            {
                // 1. Update changed projection ONLY up to regular checkpoint
                var modifiedProjections = SelectModified(projections);
                if (modifiedProjections.Any())
                {
                    Rebuild(projectionСhangeCheckpoint, regularCheckpoint, NewProjector(modifiedProjections), CheckpointScope.ProjectionChange);
                    if (running.IsCancellationRequested) return;
                    MarkAsUnmodified(modifiedProjections);
                }
            }

            // 2. Replay commit's tail for all projections
            Rebuild(regularCheckpoint, null, NewProjector(projections), CheckpointScope.Default);
            if (running.IsCancellationRequested) return;

            // 3. Replay commit's tail again to handle events which was fired during rebuild
            Rebuild(regularCheckpoint, null, NewProjector(projections), CheckpointScope.Default);
        }

        void MarkAsUnmodified(IEnumerable<IProjection> projections)
        {
            versions.Save(projections.Select(e => new ProjectionInfo { Name = e.GetType().FullName, IsExist = false }).ToArray());
        }

        private IProjection[] SelectModified(IProjection[] projections)
        {
            var version = versions.Restore(projections.Select(e => e.GetType().FullName).ToArray())
                .Where(e => e.IsExist)
                .ToDictionary(e => e.Name, e => e);
            ProjectionInfo info;
            return (from projection in projections
                    let name = projection.GetType().FullName
                    where version.TryGetValue(name, out info) && info.IsExist && info.Version == projection.Version
                    select projection).ToArray();
        }

        void Rebuild(Checkpoint? fromCheckpoint, Checkpoint? toCheckpoint, Projector projector, CheckpointScope checkpointScope)
        {
            string from = fromCheckpoint.HasValue ? fromCheckpoint.Value.Position : null;
            string to = toCheckpoint.HasValue ? toCheckpoint.Value.Position : null;
            var commits = eventStream.Read(fromCheckpoint);
            commits = FilterByCheckpoints(commits, from, to);
            commits = PauseAware(commits, from, to);
            commits = ShowLogs(commits, from, to);

            if (from == null)
                foreach (var projection in projections)
                    projection.Clear();

            foreach (var projection in projector.projections)
                projection.Begin();

            Checkpoint? processedCheckpoint = null;
            foreach (var eventsSlice in commits)
            {
                projector.Handle(eventsSlice);
                processedCheckpoint = eventsSlice.Checkpoint;
            }

            foreach (var projection in projector.projections)
                projection.Flush();

            checkpoints.Save(processedCheckpoint, checkpointScope);
        }

        IEnumerable<EventsSlice> FilterByCheckpoints(IEnumerable<EventsSlice> commits, string from, string to)
        {
            bool startFound = from == null;
            foreach (var commit in commits)
            {
                if (!startFound)
                {
                    if (from == commit.Checkpoint.Position)
                    {
                        startFound = true;
                    }
                    continue;
                }
                yield return commit;
                if (to != null && to == commit.Checkpoint.Position)
                    yield break;
            }
            if (!startFound)
                throw new Exception(string.Format("Checkpoint {0} can not be found. At the end", from));
        }

        IEnumerable<EventsSlice> PauseAware(IEnumerable<EventsSlice> commits, string from, string to)
        {
            EventsSlice? processed = null;
            foreach (var commit in commits)
            {
                yield return commit;

                processed = commit;
                if (running.IsCancellationRequested)
                    break;
            }

            if (processed.HasValue)
            {
                Checkpoint? pos = processed.Value.Checkpoint;
                var scope = (to == null) ? CheckpointScope.Default : CheckpointScope.ProjectionChange;
                if (running.IsCancellationRequested == false && scope == CheckpointScope.ProjectionChange)
                {
                    pos = null;
                }
                checkpoints.Save(pos, scope);
            }
        }

        IEnumerable<EventsSlice> ShowLogs(IEnumerable<EventsSlice> commits, string from, string to)
        {
            bool first = true;
            EventsSlice? processed = null;
            long noOfCommits = 0;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            foreach (var commit in commits)
            {
                noOfCommits++;
                if (first)
                {
                    if (from == null)
                        log.Info("Rebuild started from the beginning");
                    else
                        log.Info(string.Format("Rebuild started from {0} due to checkpoint {1}", commit.Checkpoint, from));
                    first = false;
                }

                yield return commit;

                processed = commit;
                if (noOfCommits % 1000 == 0)
                    log.Info(string.Format("{0} commits were processed {1}", noOfCommits, timer.Elapsed));
            }
            if (processed.HasValue)
            {
                log.Info(string.Format("{0} commits were processed {1:mm\\:ss\\.ff}", noOfCommits, timer.Elapsed));
                if (to == null)
                    log.Info(string.Format("Rebuild finished on the end {0}", processed.Value.Checkpoint));
                else
                    log.Info(string.Format("Rebuild finished on checkpoint {0}", to));
            }
        }

        protected virtual Projector NewProjector(IProjection[] projections)
        {
            return new Projector(projections, log);
        }
    }
}
