using System;
using NEventStore.Cqrs.Messages;

namespace PetProject.Books.Shared.Commands
{
    public class TransferAccountValue : DomainCommand
    {
        public Guid From { get; set; }
        public Guid To { get; set; }
        public decimal Value { get; set; }
    }
}
