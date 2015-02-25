using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NEventStore.Cqrs.Messages;
using NEventStore.Cqrs.Tests.Mocks;
using NUnit.Framework;

namespace NEventStore.Cqrs.Tests
{
    public abstract class HandlerTest : TestBase
    {
        protected RepositoryMock Repository { get; set; }
        protected SagaRepositoryMock SagaRepository { get; set; }
        protected IdGeneratorMock IdGenerator { get; set; }
        protected CommandBusMock CommandBus { get; set; }

        [SetUp]
        public virtual void Init()
        {
            Repository = new RepositoryMock();
            SagaRepository = new SagaRepositoryMock();
            IdGenerator = new IdGeneratorMock();
            CommandBus = new CommandBusMock();
        }

        [TearDown]
        public virtual void Cleanup()
        {
            Repository = null;
            SagaRepository = null;
            IdGenerator = null;
        }

        protected override void ClearEvents()
        {
            Repository.SavedAggregate.ClearUncommittedEvents();
        }

        protected override List<T> GetUncommitted<T>()
        {
            IEnumerable<IMessage> result = new IMessage[0];

            var aggregate = Repository.SavedAggregate;
            if (aggregate != null)
            {
                result = result
                    .Union(aggregate.GetUncommittedEvents().OfType<IMessage>());
            }

            var saga = SagaRepository.SavedSaga;
            if (saga != null)
            {
                result = result
                    .Union(saga.GetUncommittedEvents().OfType<IMessage>())
                    .Union(saga.GetUndispatchedMessages().OfType<IMessage>());
            }

            result = result.Union(CommandBus.GetCommands());

            return result.OfType<T>().ToList();
        }
        
        protected override ICollection GetUndispatchedMessages<TCommand>()
        {
            return SagaRepository.SavedSaga.GetUndispatchedMessages();
        }
    }
}
