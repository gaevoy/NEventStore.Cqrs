using System;
using System.Linq;
using EventStream.Projector;
using NEventStore.Cqrs.Messages;
using NEventStore.Dispatcher;
using Newtonsoft.Json;

namespace NEventStore.Cqrs.Impl
{
    internal class CommitDispatcher : IDispatchCommits
    {
        private readonly ICommandBus commandBus;
        private readonly IEventBus eventBus;
        private readonly GenericMethodCaller eventBusPublish;
        private readonly GenericMethodCaller commandBusPublish;
        private readonly ILogger logger;
        private readonly IProjector projector;
        private readonly object lockObject = new object();
        public CommitDispatcher(ICommandBus commandBus, IEventBus eventBus, ILogger logger, IProjector projector)
        {
            this.commandBus = commandBus;
            this.eventBus = eventBus;
            this.logger = logger;
            this.projector = projector;
            eventBusPublish = new GenericMethodCaller(eventBus, "Publish");
            commandBusPublish = new GenericMethodCaller(commandBus, "Publish");
        }

        public void Dispose()
        {

        }

        public void Dispatch(ICommit commit)
        {
            lock (lockObject)
            {
                if (commit.Headers.Keys.Any(s => s == "SagaType"))
                {
                    var commands = commit.Headers.Where(x => x.Key.StartsWith("UndispatchedMessage.")).Select(x => x.Value).ToList();

                    foreach (var cmd in commands.Cast<ICommand>())
                        try
                        {
                            commandBusPublish.Call(cmd);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, cmd);
                            eventBus.Publish(new CommandDispatchFailedEvent
                            {
                                Id = new Guid(commit.StreamId),
                                CommandType = cmd.GetType().ToString(),
                                Command = JsonConvert.SerializeObject(cmd),
                                ErrorType = ex.GetType().ToString(),
                                ErrorMessage = ex.Message
                            });
                        }
                }
                else
                {
                    foreach (var evt in commit.Events.Select(e => e.Body))
                        eventBusPublish.Call(evt);
                    projector.Handle(new EventsSlice(new Checkpoint(commit.CheckpointToken), commit.Events.Select(e => e.Body)));
                }
            }
        }
    }
}
