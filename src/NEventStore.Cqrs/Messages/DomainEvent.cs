using System;

namespace NEventStore.Cqrs.Messages
{
    public abstract class DomainEvent : IEvent
    {
        protected DomainEvent()
        {
            Created = DateTime.UtcNow;
        }
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public int Version { get; set; }
        public Guid? IssuedBy { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? SagaId { get; set; }
    }
}