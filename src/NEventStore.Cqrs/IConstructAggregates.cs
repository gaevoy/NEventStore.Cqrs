using System;
using System.Collections.Generic;
using CommonDomain;

namespace NEventStore.Cqrs
{
    public interface IConstructAggregates
    {
        IAggregate Build(Type type, Guid id, IMemento snapshot, IDictionary<string, object> headers);
    }
}
