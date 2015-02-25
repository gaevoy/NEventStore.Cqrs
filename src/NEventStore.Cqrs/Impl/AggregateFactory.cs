using System;
using System.Collections.Generic;
using System.Reflection;
using CommonDomain;

namespace NEventStore.Cqrs.Impl
{
    public class AggregateFactory : IConstructAggregates
    {
        public IAggregate Build(Type type, Guid id, IMemento snapshot, IDictionary<string, object> headers)
        {
            Type[] types = { typeof(Guid) };
            ConstructorInfo constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, types, null);
            return constructor.Invoke(new object[] { id }) as IAggregate;
        }
    }
}
