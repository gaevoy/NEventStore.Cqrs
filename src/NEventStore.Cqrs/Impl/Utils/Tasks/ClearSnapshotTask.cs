using System;
using NEventStore.Cqrs.Utils;

namespace NEventStore.Cqrs.Impl.Utils.Tasks
{
    public class ClearSnapshotTask
    {
        private readonly ILogger log;
        private readonly IPersistHelper persistHelper;

        public ClearSnapshotTask(ILogger log, IPersistHelper persistHelper)
        {
            if (log == null) throw new ArgumentNullException("log");
            this.log = log;
            this.persistHelper = persistHelper;
        }

        public void Run()
        {
            log.Info("Clear snapshots");
            persistHelper.ClearSnapshots();
        }
    }
}