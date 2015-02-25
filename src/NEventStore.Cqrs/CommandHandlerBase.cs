using CommonDomain;
using CommonDomain.Persistence;
using NEventStore.Cqrs.Messages;

namespace NEventStore.Cqrs
{
    public abstract class CommandHandlerBase
    {
        protected readonly IRepository repo;

        protected CommandHandlerBase(IRepository repo)
        {
            this.repo = repo;
        }

        protected virtual void Save(IAggregate aggr, DomainCommand by)
        {
            foreach (DomainEvent evt in aggr.GetUncommittedEvents())
            {
                if (!evt.TenantId.HasValue) evt.TenantId = @by.TenantId;
                if (!evt.IssuedBy.HasValue) evt.IssuedBy = @by.IssuedBy;
                if (!evt.SagaId.HasValue) evt.SagaId = @by.SagaId;
                evt.Version = aggr.Version;
            }
            repo.Save(aggr, @by.CommitId);
        }
    }
}
