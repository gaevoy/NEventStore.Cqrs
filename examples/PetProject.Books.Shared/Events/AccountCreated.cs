using NEventStore.Cqrs.Messages;

namespace PetProject.Books.Shared.Events
{
    public class AccountCreated : DomainEvent
    {
        public string DisplayName { get; set; }
    }
}