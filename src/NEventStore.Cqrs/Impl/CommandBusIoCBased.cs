using NEventStore.Cqrs.Messages;
using System;
using System.Diagnostics;
using System.Linq;

namespace NEventStore.Cqrs.Impl
{
    public class CommandBusIoCBased : ICommandBus
    {
        private readonly IDependencyResolver ioc;
        private readonly ILogger logger;

        public CommandBusIoCBased(IDependencyResolver ioc, ILogger logger)
        {
            this.ioc = ioc;
            this.logger = logger;
        }

        public void Publish<T>(T command) where T : class, ICommand
        {
            var handlers = ioc.ResolveAll<IHandler<T>>().ToArray();
            if (handlers.Length > 1) throw new InvalidOperationException("Can not send to more than one handler");
            if (handlers.Length == 0) throw new InvalidOperationException(string.Format("No handler registered for command {0}", command.GetType().Name));

            var duration = new Stopwatch();
            duration.Start();
            handlers[0].Handle(command);
            logger.Debug(string.Format("{0} {1}ms", command.GetType().Name, duration.ElapsedMilliseconds));
        }
    }
}
