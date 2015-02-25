using System.Collections.Generic;
using NEventStore.Cqrs.Messages;

namespace NEventStore.Cqrs.Tests.Mocks
{
    public class CommandBusMock : ICommandBus
    {
        private Queue<ICommand> _queue = new Queue<ICommand>();

        public IEnumerable<ICommand> GetCommands()
        {
            return _queue.ToArray();
        }

        public void Publish<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            _queue.Enqueue(command);
        }
    }
}