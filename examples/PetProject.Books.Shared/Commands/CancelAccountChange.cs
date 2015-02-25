using NEventStore.Cqrs.Messages;

namespace PetProject.Books.Shared.Commands
{
    public class CancelAccountChange : DomainCommand
    {
        public string Reason { get; set; }
    }
}
