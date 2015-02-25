using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommonDomain.Persistence;
using NEventStore.Cqrs.Impl;
using NEventStore.Cqrs.Projections;
using NEventStore.Cqrs.Tests.Impl;
using NEventStore.Cqrs.Tests.Mocks;
using NEventStore.Persistence;
using NEventStore.Serialization;
using NUnit.Framework;

namespace NEventStore.Cqrs.Tests
{
    public abstract class InfrastructureAwareTest : TestBase
    {
        protected InfrastructureAwareTest(Assembly[] appAssemblies)
        {
            this.appAssemblies = appAssemblies;
        }

        protected IRepository Repository { get { return innerIocLocal.Resolve<IRepository>(); } }
        protected ISagaRepository SagaRepository { get { return innerIocLocal.Resolve<ISagaRepository>(); } }
        protected IdGeneratorMock IdGenerator { get; set; }
        protected ICommandBus CommandBus { get { return innerIocLocal.Resolve<ICommandBus>(); } }

        private readonly Assembly[] appAssemblies = new Assembly[0];
        private PublishedEventSniffer eventBus;
        private DictionaryBasedDependencyResolver ioc = null;
        private NanoContainer innerIocLocal = null;

        [SetUp]
        public virtual void Init()
        {
            ioc = new DictionaryBasedDependencyResolver();
            IdGenerator = new IdGeneratorMock();

            var wireup = Wireup.Init()
                .UsingInMemoryPersistence()
                    .UsingJsonSerialization()
                .UsingEventUpconversion()
                    .WithConvertersFrom(appAssemblies)
                .UsingSynchronousDispatchScheduler()
                .UsingCqrs(ioc).WithAggregateFactory(_ => new AggregateFactoryHeaderBased(appAssemblies));
            
            wireup.Hook(innerIoc =>
            {
                eventBus = new PublishedEventSniffer(innerIoc.Resolve<IEventBus>());
                innerIoc.Register<IIdGenerator>(IdGenerator);
                innerIoc.Register<IEventBus>(eventBus);
                innerIoc.Register<ICheckpointStore>(new CheckpointStoreMock());
                innerIoc.Register<IPersistStreams>(new InMemoryPersistenceEngineWithSerialization(innerIoc.Resolve<ISerialize>()));
                innerIocLocal = innerIoc;
            });
            OnConfigure(ioc, innerIocLocal);
            wireup.Build();
        }

        protected virtual void OnConfigure(IDependencyResolver ioc, NanoContainer innerIoc) { }

        protected override void ClearEvents()
        {
            eventBus.PublishedEvents.Clear();
        }

        protected override List<T> GetUncommitted<T>()
        {
            return eventBus.PublishedEvents.OfType<T>().ToList();
        }

        protected override ICollection GetUndispatchedMessages<TCommand>()
        {
            throw new NotImplementedException();
        }

        protected void RegisterHandlers<T>(Func<T> handler) where T : class
        {
            ioc.RegisterAll(handler, of: typeof(IHandler<>));
        }
    }
}
