using System;
using System.Diagnostics;
using System.Linq;
using NEventStore.Cqrs.Messages;

namespace NEventStore.Cqrs.Impl
{
    public class EventBusIoCBased : IEventBus
    {
        private readonly IDependencyResolver ioc;
        private readonly ILogger logger;

        public EventBusIoCBased(IDependencyResolver ioc, ILogger logger)
        {
            this.ioc = ioc;
            this.logger = logger;
        }

        public void Publish<T>(T evt) where T : class, IEvent
        {
            var duration = new Stopwatch();
            duration.Start();
            var handlers = ioc.ResolveAll<IHandler<T>>().ToList();
            foreach (IHandler<T> handler in handlers)
                try
                {
                    handler.Handle(evt);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, evt);
                }
            logger.Debug(string.Format("{0} {1}ms", evt.GetType().Name, duration.ElapsedMilliseconds));
        }
    }
}
