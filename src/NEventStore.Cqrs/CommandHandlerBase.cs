using CommonDomain;
using CommonDomain.Persistence;
using NEventStore.Cqrs.Messages;

namespace NEventStore.Cqrs
{
    public abstract class CommandHandlerBase
    {
        protected readonly IRepository repo;
        internal delegate void OnAggregateSaved(CommandHandlerBase sender, IAggregate aggr, DomainCommand by);
        /// <summary>
        /// Don't use it, this is here only for building Aggregate interaction graph
        /// </summary>
        internal static OnAggregateSaved OnSavedHook;

        protected CommandHandlerBase(IRepository repo)
        {
            this.repo = repo;
        }

        protected virtual void Save(IAggregate aggr, DomainCommand by)
        {
            if (OnSavedHook != null) OnSavedHook(this, aggr, by);
            foreach (DomainEvent evt in aggr.GetUncommittedEvents())
            {
                if (!evt.TenantId.HasValue) evt.TenantId = @by.TenantId;
                if (!evt.IssuedBy.HasValue) evt.IssuedBy = @by.IssuedBy;
                if (!evt.SagaId.HasValue) evt.SagaId = @by.SagaId;
                evt.Version = aggr.Version;
            }
            repo.Save(aggr, @by.CommitId);
        }

        protected virtual void Save(IAggregate aggr, DomainCommand by, string bucketId)
        {
            if (OnSavedHook != null) OnSavedHook(this, aggr, by);
            foreach (DomainEvent evt in aggr.GetUncommittedEvents())
            {
                if (!evt.TenantId.HasValue) evt.TenantId = @by.TenantId;
                if (!evt.IssuedBy.HasValue) evt.IssuedBy = @by.IssuedBy;
                if (!evt.SagaId.HasValue) evt.SagaId = @by.SagaId;
                evt.Version = aggr.Version;
            }
            repo.Save(bucketId, aggr, @by.CommitId);
        }
    }
}
