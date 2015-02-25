using NEventStore.Cqrs.Messages;

namespace NEventStore.Cqrs
{
    public interface ICommandBus
    {
        void Publish<TCommand>(TCommand command) where TCommand: class, ICommand;
    }
}
