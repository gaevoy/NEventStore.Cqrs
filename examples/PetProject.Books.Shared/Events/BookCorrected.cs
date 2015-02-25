using NEventStore.Cqrs.Messages;

namespace PetProject.Books.Shared.Events
{
    public class BookCorrected : DomainEvent
    {
        public string Title { get; set; }
    }
}