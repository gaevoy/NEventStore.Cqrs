using System;

namespace NEventStore.Cqrs.Messages
{
    public abstract class DomainCommand : ICommand
    {
        protected DomainCommand()
        {
            CommitId = Guid.NewGuid();
            Version = int.MaxValue;
        }

        public Guid Id { get; set; }
        public int Version { get; set; }
        public Guid CommitId { get; set; }
        public Guid? IssuedBy { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? SagaId { get; set; }
    }
}
