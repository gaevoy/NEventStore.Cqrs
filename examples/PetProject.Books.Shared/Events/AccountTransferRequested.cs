using System;
using NEventStore.Cqrs.Messages;

namespace PetProject.Books.Shared.Events
{
    public class AccountTransferRequested : DomainEvent
    {
        public Guid From { get; set; }
        public Guid To { get; set; }
        public decimal Value { get; set; }
    }
}