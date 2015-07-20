using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using EventStream.Projector.Logger;
using EventStream.Projector.Persistence;

namespace EventStream.Projector.Impl
{
    public class ProjectionRebuild : IProjectionRebuild
    {
        private readonly IProjection[] projections;
        private readonly IProjectionInfoStore versions;
        private readonly ILog log;
        private readonly ICheckpointStore checkpoints;

        public ProjectionRebuild(IProjection[] projections, ICheckpointStore checkpoints, IProjectionInfoStore versions, ILog log = null)
        {
            this.projections = projections;
            this.versions = versions;
            this.log = log ?? new NullLog();
            this.checkpoints = checkpoints;
        }

        public void Start(IEventStream eventStream, CancellationToken running)
        {
            if (running.IsCancellationRequested) return;

            var projectionСhangeCheckpoint = checkpoints.Restore(Checkpoint.ProjectionChange);
            var regularCheckpoint = checkpoints.Restore(Checkpoint.Default);

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
                    Rebuild(eventStream, ref projectionСhangeCheckpoint, regularCheckpoint, NewProjector(modifiedProjections), running);
                    if (running.IsCancellationRequested) return;
                    MarkAsUnmodified(modifiedProjections);
                }
            }

            // 2. Replay commit's tail for all projections
            Rebuild(eventStream, ref regularCheckpoint, null, NewProjector(projections), running);
            if (running.IsCancellationRequested) return;

            // 3. Replay commit's tail again to handle events which was fired during rebuild
            Rebuild(eventStream, ref regularCheckpoint, null, NewProjector(projections), running);
        }

        public string GetChangedProjectionsInfo(bool showEmptyInfo)
        {
            var text = new StringBuilder();

            var info = versions.Restore(projections);

            if (info.All(e => !e.IsExist))
            {
                text.AppendLine("Information about version is absent. All projections will be rebuilt");
            }
            else
            {
                var changed = info.Where(e => e.Projection.Version != e.Version || e.IsExist == false).ToArray();
                if (changed.Any())
                {
                    text.AppendFormat("Changed projections to rebuild: ").AppendLine();
                    foreach (var projectionInfo in changed)
                        text.AppendLine(projectionInfo.Projection.GetType().FullName);
                }
                else if (showEmptyInfo)
                {
                    text.Append("Nothing to rebuild");
                }
            }
            return text.ToString();
        }

        public bool IsRequired()
        {
            return versions.Restore(projections).Any(e => e.Projection.Version != e.Version || e.IsExist == false);
        }

        void Rebuild(IEventStream eventStream, ref Checkpoint? from, Checkpoint? to, SimpleProjector projector, CancellationToken running)
        {
            var commits = eventStream.Read(from);
            commits = FilterByCheckpoints(commits, from, to);
            commits = PauseAware(commits, from, to, running);
            commits = ShowLogs(commits, from, to);

            if (from == null)
                foreach (var projection in projections)
                    projection.Clear();

            foreach (var projection in projector.projections)
                projection.Begin();

            foreach (var eventsSlice in commits)
            {
                projector.Handle(eventsSlice);
                from = eventsSlice.Checkpoint;
            }

            foreach (var projection in projector.projections)
                projection.Flush();
        }

        IEnumerable<EventsSlice> FilterByCheckpoints(IEnumerable<EventsSlice> commits, Checkpoint? from, Checkpoint? to)
        {
            foreach (var commit in commits)
            {
                yield return commit;
                if (to == commit.Checkpoint)
                    yield break;
            }
        }

        IEnumerable<EventsSlice> PauseAware(IEnumerable<EventsSlice> commits, Checkpoint? from, Checkpoint? to, CancellationToken running)
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
                var scope = (to == null) ? Checkpoint.Default : Checkpoint.ProjectionChange;
                if (running.IsCancellationRequested == false && scope == Checkpoint.ProjectionChange)
                    pos = null;
                checkpoints.Save(pos, scope);
            }
        }

        IEnumerable<EventsSlice> ShowLogs(IEnumerable<EventsSlice> commits, Checkpoint? from, Checkpoint? to)
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


        void MarkAsUnmodified(IEnumerable<IProjection> projections)
        {
            versions.Save(projections.Select(e => new ProjectionInfo(projection: e, version: e.Version, isExist: true)).ToArray());
        }

        IProjection[] SelectModified(IProjection[] projections)
        {
            return versions.Restore(projections)
                .Where(e => e.Projection.Version != e.Version || e.IsExist == false)
                .Select(e => e.Projection)
                .ToArray();
        }

        protected virtual SimpleProjector NewProjector(IProjection[] projections)
        {
            return new SimpleProjector(projections, log: log);
        }
    }
}
