using System;
using System.Collections.Generic;
using CommonDomain.Core;
using CommonDomain.Persistence;
using NEventStore.Cqrs.Utils;

namespace NEventStore.Cqrs.Impl.Utils.Tasks
{
    public class CheckEventStoreTask
    {
        private readonly ILogger log;
        private readonly IRepository repository;
        private readonly IPersistHelper persistHelper;

        public CheckEventStoreTask(ILogger log, IRepository repository, IPersistHelper persistHelper)
        {
            this.log = log;
            this.repository = repository;
            this.persistHelper = persistHelper;
        }

        public void Run()
        {
            log.Info("Check eventstore");
            CheckAgregates();
        }

        private void CheckAgregates()
        {
            var ids = persistHelper.GetIdsOfAggregates();
            var errors = new List<Exception>();

            var i = 0;
            DateTime time = DateTime.Now;
            foreach (var id in ids)
            {
                i++;

                try
                {
                    repository.GetById<AggregateBase>(id);
                }
                catch (Exception ex)
                {
                    var error = new Exception(string.Format("Cannot get aggregate '{0}' from repository.", id), ex);
                    errors.Add(error);
                    log.Error(ex);
                }

                var duration = DateTime.Now - time;

                if (duration > TimeSpan.FromSeconds(1))
                {
                    int percentage = (int)Math.Round(i * 100 / (float)ids.Count);
                    log.Info(string.Format("{0} %", percentage));
                    time = DateTime.Now;
                }
            }

            if (errors.Count > 0)
            {
                throw new Exception(string.Format("Event log has errors. Error count: '{0}'", errors.Count));
            }
        }
    }
}