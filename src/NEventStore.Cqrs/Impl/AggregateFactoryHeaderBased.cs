using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommonDomain;

namespace NEventStore.Cqrs.Impl
{
    public class AggregateFactoryHeaderBased : IConstructAggregates
    {
        readonly Assembly[] domainAssemblies;
        readonly IConstructAggregates factory;
        readonly ConcurrentDictionary<string, Type> types = new ConcurrentDictionary<string, Type>();

        public AggregateFactoryHeaderBased(IConstructAggregates factory, params Assembly[] domainAssemblies)
        {
            this.factory = factory;
            this.domainAssemblies = domainAssemblies;
        }
        public AggregateFactoryHeaderBased(params Assembly[] domainAssemblies)
            : this(new AggregateFactory(), domainAssemblies)
        {
        }

        public IAggregate Build(Type type, Guid id, IMemento snapshot, IDictionary<string, object> headers)
        {
            Type concreteType;
            object concreteTypeName;
            if (headers.TryGetValue(EventStoreRepository.AggregateTypeHeader, out concreteTypeName))
            {
                string typeName = (string)concreteTypeName;
                if (!types.TryGetValue(typeName, out concreteType))
                {
                    concreteType = domainAssemblies.Select(a => a.GetType(typeName)).First(t => t != null);
                    if (concreteType == null) throw new TypeLoadException(string.Format("Type {0} is not found", typeName));
                    types[typeName] = concreteType;
                }
            }
            else
            {
                concreteType = type;
            }
            return factory.Build(concreteType, id, snapshot, headers);
        }

        public AggregateFactoryHeaderBased Register(string aggregateTypeName, Type aggregateType)
        {
            types[aggregateTypeName] = aggregateType;
            return this;
        }
        public AggregateFactoryHeaderBased Register<TAggregate>(string aggregateTypeName) where TAggregate : class, IAggregate
        {
            return Register(aggregateTypeName, typeof(TAggregate));
        }
    }
}
