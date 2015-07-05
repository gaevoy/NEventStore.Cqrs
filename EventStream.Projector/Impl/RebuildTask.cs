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
        private readonly IProjectionVersioning versions;
        private readonly IEventStream eventStream;
        private readonly ILog log;
        private readonly ICheckpoint checkpoint;
        private readonly CancellationToken running;

        public RebuildTask(IProjection[] projections, IProjectionVersioning versions, IEventStream eventStream, ILog log, ICheckpoint checkpoint, CancellationToken running)
        {
            this.projections = projections;
            this.versions = versions;
            this.eventStream = eventStream;
            this.log = log;
            this.checkpoint = checkpoint;
            this.running = running;
        }

        public void Start()
        {
            if (running.IsCancellationRequested) return;

            var projectionСhangeCheckpoint = checkpoint.Restore(CheckpointScope.ProjectionChange);
            var regularCheckpoint = checkpoint.Restore(CheckpointScope.Regular);

            // If regular checkpoint is undefined rebuild all projections
            if (regularCheckpoint == null)
            {
                versions.MarkAsUnmodified(projections);
            }
            else
            {
                // 1. Update changed projection ONLY up to regular checkpoint
                var modifiedProjections = versions.SelectModified(projections);
                if (modifiedProjections.Any())
                {
                    Rebuild(projectionСhangeCheckpoint, regularCheckpoint, NewProjector(modifiedProjections, CheckpointScope.ProjectionChange));
                    if (running.IsCancellationRequested) return;
                    versions.MarkAsUnmodified(modifiedProjections);
                }
            }

            // 2. Replay commit's tail for all projections
            Rebuild(regularCheckpoint, null, NewProjector(projections, CheckpointScope.Regular));
            if (running.IsCancellationRequested) return;

            // 3. Replay commit's tail again to handle events which was fired during rebuild
            Rebuild(regularCheckpoint, null, NewProjector(projections, CheckpointScope.Regular));
        }

        void Rebuild(string from, string to, IProjector projector)
        {
            var commits = eventStream.Read(from);
            commits = FilterByCheckpoints(commits, from, to);
            commits = PauseAware(commits, from, to);
            commits = ShowLogs(commits, from, to);

            if (from == null)
                foreach (var projection in projections)
                    projection.Clear();

            projector.HandleAll(commits);
        }

        IEnumerable<EventsSlice> FilterByCheckpoints(IEnumerable<EventsSlice> commits, string from, string to)
        {
            bool startFound = from == null;
            foreach (var commit in commits)
            {
                if (!startFound)
                {
                    if (from == commit.Position)
                    {
                        startFound = true;
                    }
                    continue;
                }
                yield return commit;
                if (to != null && to == commit.Position)
                    yield break;
            }
            if (!startFound)
                throw new Exception(string.Format("Checkpoint {0} can not be found. At the end", from));
        }

        IEnumerable<EventsSlice> PauseAware(IEnumerable<EventsSlice> commits, string from, string to)
        {
            EventsSlice processed = null;
            foreach (var commit in commits)
            {
                yield return commit;

                processed = commit;
                if (running.IsCancellationRequested)
                    break;
            }

            if (processed != null)
            {
                var pos = processed.Position;
                var scope = (to == null) ? CheckpointScope.Regular : CheckpointScope.ProjectionChange;
                if (running.IsCancellationRequested == false && scope == CheckpointScope.ProjectionChange)
                {
                    pos = null;
                }
                checkpoint.Save(pos, scope);
            }
        }

        IEnumerable<EventsSlice> ShowLogs(IEnumerable<EventsSlice> commits, string from, string to)
        {
            bool first = true;
            EventsSlice processed = null;
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
                        log.Info(string.Format("Rebuild started from {0} due to checkpoint {1}", commit.Position, from));
                    first = false;
                }

                yield return commit;

                processed = commit;
                if (noOfCommits % 1000 == 0)
                    log.Info(string.Format("{0} commits were processed {1}", noOfCommits, timer.Elapsed));
            }
            if (processed != null)
            {
                log.Info(string.Format("{0} commits were processed {1:mm\\:ss\\.ff}", noOfCommits, timer.Elapsed));
                if (to == null)
                    log.Info(string.Format("Rebuild finished on the end {0}", processed.Position));
                else
                    log.Info(string.Format("Rebuild finished on checkpoint {0}", to));
            }
        }

        protected virtual Projector NewProjector(IProjection[] projections, CheckpointScope checkpointScope)
        {
            return new Projector(projections, log, checkpoint, checkpointScope);
        }
    }
}
