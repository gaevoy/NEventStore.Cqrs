using NEventStore.Cqrs.Messages;

namespace PetProject.Books.Shared.Events
{
    public class BookRegistered : DomainEvent
    {
        public string Title { get; set; }
        public string ISBN { get; set; }
    }
}
