using System;
using System.Linq;
using NEventStore.Cqrs.Projections;

namespace NEventStore.Cqrs.Impl.Utils.Tasks
{
    public class ShowChangedProjectionsTask
    {
        private readonly ILogger log;
        private readonly IDependencyResolver ioc;
        private readonly IVersioningRepository versioningRepository;
        private readonly string rmConnectionString;

        public ShowChangedProjectionsTask(ILogger log, IDependencyResolver ioc, IVersioningRepository versioningRepository, string rmConnectionString)
        {
            if (log == null) throw new ArgumentNullException("log");
            if (ioc == null) throw new ArgumentNullException("ioc");
            if (versioningRepository == null) throw new ArgumentNullException("versioningRepository");
            this.log = log;
            this.ioc = ioc;
            this.versioningRepository = versioningRepository;
            this.rmConnectionString = rmConnectionString;
        }

        public void Run()
        {
            log.Info("Showing all changed projections. Connection string: " + rmConnectionString);
            IProjection[] projections = ioc.ResolveAll<IProjection>().ToArray();

            if (versioningRepository.IsInitialized(projections))
            {
                projections = versioningRepository.SelectModified(projections);
                foreach (IProjection projection in projections)
                {
                    log.Info(projection.GetType().FullName + ": " + versioningRepository.GetModifiedReason(projection));
                }
                if (!projections.Any())
                {
                    log.Info("Nothing to rebuild");
                }
            }
            else
            {
                log.Info("Information about version is absent. All projections will be rebuilt");
            }
        }
    }
}