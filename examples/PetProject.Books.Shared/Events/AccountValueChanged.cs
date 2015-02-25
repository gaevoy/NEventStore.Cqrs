using NEventStore.Cqrs.Messages;

namespace PetProject.Books.Shared.Events
{
    public class AccountValueChanged : DomainEvent
    {
        public decimal ValueDifference { get; set; }
        public string Reason { get; set; }
    }
}