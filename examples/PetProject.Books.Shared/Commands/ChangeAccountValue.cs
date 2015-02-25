using NEventStore.Cqrs.Messages;

namespace PetProject.Books.Shared.Commands
{
    public class ChangeAccountValue : DomainCommand
    {
        public decimal Value { get; set; }
    }
}
