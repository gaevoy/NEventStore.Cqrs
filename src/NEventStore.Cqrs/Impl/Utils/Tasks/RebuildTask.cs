using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NEventStore.Cqrs.Messages;
using NEventStore.Cqrs.Projections;
using NEventStore.Cqrs.Utils.History;
using NEventStore.Persistence;

namespace NEventStore.Cqrs.Impl.Utils.Tasks
{
    public class RebuildTask
    {
        private readonly ILogger log;
        private readonly IDependencyResolver ioc;
        private readonly IHistoryReader historyReader;
        private readonly IVersioningRepository versioningRepository;
        private readonly ICheckpointStore checkpoints;
        private readonly CancellationToken running;

        public RebuildTask(ILogger log,
                           IDependencyResolver ioc,
                           IHistoryReader historyReader,
                           IVersioningRepository versioningRepository,
                           ICheckpointStore checkpoints,
                           CancellationToken? pause = null)
        {
            if (log == null) throw new ArgumentNullException("log");
            if (ioc == null) throw new ArgumentNullException("ioc");
            if (historyReader == null) throw new ArgumentNullException("historyReader");
            if (versioningRepository == null) throw new ArgumentNullException("versioningRepository");
            if (checkpoints == null) throw new ArgumentNullException("checkpoints");
            this.log = log;
            this.ioc = ioc;
            this.historyReader = historyReader;
            this.versioningRepository = versioningRepository;
            this.checkpoints = checkpoints;
            this.running = pause ?? CancellationToken.None;
        }

        public void Run()
        {
            if (running.IsCancellationRequested) return;

            Checkpoint projectionСhangeCheckpoint = checkpoints.Load(Checkpoint.PROJECTION_CHANGE);
            Checkpoint regularCheckpoint = checkpoints.Load(Checkpoint.REGULAR);

            IProjection[] projections = ioc.ResolveAll<IProjection>().ToArray();
            // If regular checkpoint is undefined rebuild all projections
            if (regularCheckpoint.IsUndefined)
            {
                MarkAsUnmodified(projections);
            }
            else
            {
                // 1. Update changed projection ONLY up to regular checkpoint
                var modifiedProjections = versioningRepository.SelectModified(projections);
                if (modifiedProjections.Any())
                {
                    ShowProjectionsToRebuild(modifiedProjections);
                    Rebuild(projectionСhangeCheckpoint, regularCheckpoint, modifiedProjections);
                    if (running.IsCancellationRequested) return;
                    MarkAsUnmodified(modifiedProjections);
                }
            }

            // 2. Replay commit's tail for all projections
            Rebuild(regularCheckpoint, new Checkpoint(Checkpoint.REGULAR), projections);
            if (running.IsCancellationRequested) return;

            // 3. Replay commit's tail again to handle events which was fired during rebuild
            Rebuild(regularCheckpoint, new Checkpoint(Checkpoint.REGULAR), projections);
        }

        public void ResetCheckpoints()
        {
            checkpoints.Save(new Checkpoint(Checkpoint.PROJECTION_CHANGE));
            checkpoints.Save(new Checkpoint(Checkpoint.REGULAR));
        }

        private void ShowProjectionsToRebuild(IProjection[] modifiedProjections)
        {
            log.Info("Next projections will be rebuilt:");
            foreach (IProjection projection in modifiedProjections)
                log.Info(projection.GetType().FullName);
        }

        private void MarkAsUnmodified(IEnumerable<IProjection> projections)
        {
            foreach (var projection in projections)
            {
                versioningRepository.MarkAsUnmodified(projection);
            }
        }

        private void Rebuild(Checkpoint from, Checkpoint to, IEnumerable<IProjection> rawProjections)
        {
            var projections = rawProjections.Select(e => new ProjectionWrapper(e, log)).ToArray();
            if (!projections.Any())
            {
                return;
            }

            if (from.IsUndefined)
            {
                foreach (var projection in projections)
                    projection.Clear();
            }

            DateTime start;
            if (from.IsUndefined)
            {
                start = new DateTime(2013, 1, 1);
            }
            else
            {
                start = from.CommitStampProcessed.Value.AddSeconds(-2);
            }

            foreach (var projection in projections)
                projection.Begin();

            var commits = historyReader.Read(start, DateTime.UtcNow);
            commits = FilterByCheckpoints(commits, from, to);
            commits = PauseAware(commits, from);
            commits = ShowLogs(commits, from, to, projections);

            foreach (var commit in commits)
                RebuildCommit(commit, projections);

            foreach (var projection in projections)
                projection.Flush();
        }

        private IEnumerable<Commit> FilterByCheckpoints(IEnumerable<Commit> commits, Checkpoint from, Checkpoint to)
        {
            bool startFound = from.IsUndefined;
            foreach (var commit in commits)
            {
                if (!startFound)
                {
                    if (from.IsProcessed(commit))
                    {
                        startFound = true;
                    }
                    continue;
                }
                yield return commit;
                if (!to.IsUndefined && to.IsProcessed(commit))
                    yield break;
            }
            if (!startFound)
                throw new Exception(string.Format("Checkpoint {0} {1} can not be found. At the end", from.CommitIdProcessed, from.CommitStampProcessed));
        }

        private IEnumerable<Commit> PauseAware(IEnumerable<Commit> commits, Checkpoint from)
        {
            ICommit commitProcessed = null;
            foreach (var commit in commits)
            {
                if (commit.Headers.ContainsKey("SagaType"))
                    continue;

                yield return commit;

                commitProcessed = commit;
                if (running.IsCancellationRequested)
                    break;
            }

            if (commitProcessed != null)
            {
                from.Set(commitProcessed);
                if (running.IsCancellationRequested == false && from.Mode == Checkpoint.PROJECTION_CHANGE)
                {
                    from.Set(null);
                }
                checkpoints.Save(from);
            }
        }

        private IEnumerable<Commit> ShowLogs(IEnumerable<Commit> commits, Checkpoint from, Checkpoint to, ProjectionWrapper[] projections)
        {
            bool first = true;
            ICommit commitProcessed = null;
            long noOfCommits = 0;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            foreach (var commit in commits)
            {
                noOfCommits++;
                if (first)
                {
                    if (from.IsUndefined)
                        log.Info("Rebuild started from the beginning");
                    else
                        log.Info(string.Format("Rebuild started from {0} due to checkpoint {1}", commit.CommitId, from));
                    first = false;
                }

                yield return commit;

                commitProcessed = commit;
                if (noOfCommits % 1000 == 0)
                    log.Info(string.Format("{0} commits were processed {1}", noOfCommits, timer.Elapsed));
                if (noOfCommits % 10000 == 0)
                    log.Debug("Projection statistics" + Environment.NewLine + ProjectionWrapper.GetStatistic(projections, take: 3));
            }
            if (commitProcessed != null)
            {
                log.Info(string.Format("{0} commits were processed {1:mm\\:ss\\.ff}", noOfCommits, timer.Elapsed));
                if (to.IsUndefined)
                    log.Info(string.Format("Rebuild finished on the end {0}", commitProcessed.CommitId));
                else
                    log.Info(string.Format("Rebuild finished on checkpoint {0}", to));
                log.Info("Projection statistics" + Environment.NewLine + ProjectionWrapper.GetStatistic(projections, take: 20, takeEvents: 5));
            }
        }

        private void RebuildCommit(ICommit commit, ProjectionWrapper[] projections)
        {
            foreach (var evt in commit.Events.Select(e => e.Body).OfType<IEvent>())
                foreach (var projection in projections)
                    projection.Handle(evt);
        }
    }
}