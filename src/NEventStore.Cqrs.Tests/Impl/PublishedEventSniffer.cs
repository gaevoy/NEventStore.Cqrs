using System.Collections.Generic;
using NEventStore.Cqrs.Messages;

namespace NEventStore.Cqrs.Tests.Impl
{
    public class PublishedEventSniffer : IEventBus
    {
        private readonly IEventBus underlyingEventBus;
        public List<IEvent> PublishedEvents = new List<IEvent>();

        public PublishedEventSniffer(IEventBus underlyingEventBus)
        {
            this.underlyingEventBus = underlyingEventBus;
        }

        public void Publish<TEvent>(TEvent evt) where TEvent : class, IEvent
        {
            underlyingEventBus.Publish(evt);
            lock (PublishedEvents) PublishedEvents.Add(evt);
        }
    }
}