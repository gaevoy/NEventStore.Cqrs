using NEventStore.Cqrs.Messages;

namespace NEventStore.Cqrs
{
    public interface IEventBus
    {
        void Publish<TEvent>(TEvent evt) where TEvent: class, IEvent;
    }
}
