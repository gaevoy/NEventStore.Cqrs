using NEventStore.Cqrs.Messages;

namespace NEventStore.Cqrs.Impl
{
    public class NullBus : ICommandBus, IEventBus
    {
        public void Publish<TEvent>(TEvent evt) where TEvent : class, IEvent
        {
        }

        void ICommandBus.Publish<TCommand>(TCommand command)
        {
        }
    }
}
