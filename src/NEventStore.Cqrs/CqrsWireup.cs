using System;
using CommonDomain;
using CommonDomain.Core;
using CommonDomain.Persistence;
using EventStream.Projector;
using EventStream.Projector.Persistence;
using NEventStore.Cqrs.Impl;
using NEventStore.Cqrs.Utils;
using NEventStore.Dispatcher;

namespace NEventStore.Cqrs
{
    public class CqrsWireup : Wireup
    {
        protected CqrsWireup(NanoContainer container)
            : base(container)
        {
        }

        internal protected CqrsWireup(Wireup inner, IDependencyResolver externalContainer, IProjector projector, bool enableWriteSide = true, bool enableReadSide = true)
            : base(inner)
        {
            Register<IDependencyResolver>(_ => externalContainer);
            Register<ILogger>(_ => new NullLogger());

            if (enableWriteSide)
            {
                Register<IIdGenerator>(_ => new SequentialIdgenerator());
                Register<IConstructAggregates>(_ => new AggregateFactory());
                Register<IDetectConflicts>(_ => new ConflictDetector());
                Register<IRepository>(ioc => new Impl.EventStoreRepository(
                        ioc.Resolve<IStoreEvents>(),
                        ioc.Resolve<IConstructAggregates>(),
                        ioc.Resolve<IDetectConflicts>()));
                Register<ISagaRepository>(ioc => new Impl.SagaEventStoreRepository(ioc.Resolve<IStoreEvents>()));
                Register<IDispatchCommits>(ioc => new CommitDispatcher(ioc.Resolve<ICommandBus>(), ioc.Resolve<IEventBus>(), ioc.Resolve<ILogger>(), projector));
                Register<ICommandBus>(ioc => new CommandBusIoCBased(externalContainer, ioc.Resolve<ILogger>()));
                if (enableReadSide == false)
                {
                    Register<IEventBus>(_ => new NullBus());
                }
            }
            if (enableReadSide)
            {
                if (enableWriteSide == false)
                {
                    Register<ICommandBus>(_ => new NullBus());
                    Register<IRepository>(_ => new NullRepository());
                }
                Register<IEventBus>(ioc => new EventBusIoCBased(externalContainer, ioc.Resolve<ILogger>()));
            }

            // export outside
            externalContainer.Register(_ => Container.Resolve<IRepository>());
            externalContainer.Register(_ => Container.Resolve<ISagaRepository>());
            externalContainer.Register(_ => Container.Resolve<IIdGenerator>());
            externalContainer.Register(_ => Container.Resolve<ICommandBus>());
            externalContainer.Register(_ => Container.Resolve<IEventBus>());
            externalContainer.Register(_ => Container.Resolve<IUtilityTasks>());
        }

        public CqrsWireup WithLogger(Func<NanoContainer, ILogger> ctor)
        {
            RegisterSingleton(ctor);
            return this;
        }
        public CqrsWireup WithAggregateFactory(Func<NanoContainer, IConstructAggregates> ctor)
        {
            RegisterSingleton(ctor);
            return this;
        }

        void Register<T>(Func<NanoContainer, T> func) where T : class
        {
            Container.Register(func).InstancePerCall();
        }
        void RegisterSingleton<T>(Func<NanoContainer, T> func) where T : class
        {
            Container.Register(func);
        }
    }
}
