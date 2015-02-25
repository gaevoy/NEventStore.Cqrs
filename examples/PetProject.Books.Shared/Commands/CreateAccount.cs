using NEventStore.Cqrs.Messages;

namespace PetProject.Books.Shared.Commands
{
    public class CreateAccount : DomainCommand
    {
        public string DisplayName { get; set; }
        public decimal Value { get; set; }
    }
}
