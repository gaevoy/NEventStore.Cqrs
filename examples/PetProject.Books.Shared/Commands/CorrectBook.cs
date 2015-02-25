using NEventStore.Cqrs.Messages;

namespace PetProject.Books.Shared.Commands
{
    public class CorrectBook : DomainCommand
    {
        public string Title { get; set; }
    }
}
