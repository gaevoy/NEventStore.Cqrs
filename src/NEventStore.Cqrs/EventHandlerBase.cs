using System;
using CommonDomain;
using CommonDomain.Persistence;
using NEventStore.Cqrs.Messages;

namespace NEventStore.Cqrs
{
    public abstract class EventHandlerBase
    {
        protected readonly IRepository repo;
        protected readonly ISagaRepository sagas;
        internal delegate void OnSagaSaved(EventHandlerBase sender, ISaga saga, IEvent by);
        /// <summary>
        /// Don't use it, this is here only for building Saga interaction graph
        /// </summary>
        internal static OnSagaSaved OnSavedHook;

        protected EventHandlerBase(ISagaRepository sagas)
        {
            this.sagas = sagas;
        }

        protected EventHandlerBase(IRepository repo)
        {
            this.repo = repo;
        }

        protected virtual void Save(ISaga saga, IEvent by, string bucketId = Bucket.Default)
        {
            if (string.IsNullOrWhiteSpace(saga.Id)) throw new ApplicationException("Saga.Id is not specified");
            if (OnSavedHook != null) OnSavedHook(this, saga, by);
            var byTyped = by as DomainEvent;
            foreach (IEvent e in saga.GetUncommittedEvents())
            {
                var evt = e as DomainEvent;
                if (evt != null)
                {
                    evt.SagaId = new Guid(saga.Id);

                    if (byTyped != null)
                    {
                        if (!evt.TenantId.HasValue) evt.TenantId = byTyped.TenantId;
                        if (!evt.IssuedBy.HasValue) evt.IssuedBy = byTyped.IssuedBy;
                    }
                }
            }
            foreach (DomainCommand cmd in saga.GetUndispatchedMessages())
            {
                cmd.SagaId = new Guid(saga.Id);

                if (byTyped != null)
                {
                    if (!cmd.TenantId.HasValue) cmd.TenantId = byTyped.TenantId;
                    if (!cmd.IssuedBy.HasValue) cmd.IssuedBy = byTyped.IssuedBy;
                }
            }
            sagas.Save(bucketId, saga, Guid.NewGuid(), _ => { });
        }

        protected virtual void Save(IAggregate aggr, DomainEvent by)
        {
            foreach (DomainEvent evt in aggr.GetUncommittedEvents())
            {
                if (!evt.TenantId.HasValue) evt.TenantId = @by.TenantId;
                if (!evt.IssuedBy.HasValue) evt.IssuedBy = @by.IssuedBy;
                if (!evt.SagaId.HasValue) evt.SagaId = @by.SagaId;
                evt.Version = aggr.Version;
            }
            repo.Save(aggr, Guid.NewGuid());
        }
    }
}
