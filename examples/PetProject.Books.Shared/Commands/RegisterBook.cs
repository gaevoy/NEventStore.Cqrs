using NEventStore.Cqrs.Messages;

namespace PetProject.Books.Shared.Commands
{
    public class RegisterBook : DomainCommand
    {
        public string Title { get; set; }
        public string ISBN { get; set; }
    }
}
