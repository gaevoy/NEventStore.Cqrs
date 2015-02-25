using System;
using System.Collections.Generic;
using CommonDomain;
using CommonDomain.Core;
using CommonDomain.Persistence;
using NEventStore.Cqrs.Impl;

namespace NEventStore.Cqrs.Tests.Mocks
{
    public class RepositoryMock : IRepository
    {
        public IAggregate SavedAggregate { get; set; }
        private Dictionary<Guid, object> AggregatesToGet = new Dictionary<Guid, object>();

        public TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate
        {
            object aggregate;
            if (AggregatesToGet.TryGetValue(id, out aggregate))
            {
                if (aggregate is TAggregate)
                {
                    return (TAggregate)aggregate;
                }
                else
                {
                    throw new Exception("Wrong type to get");
                }
            }
            else
            {
                throw new Exception("Aggregate is not found");
            }
        }

        public TAggregate GetById<TAggregate>(Guid id, int version) where TAggregate : class, IAggregate
        {
            return GetById<TAggregate>(id);
        }

        public TAggregate GetById<TAggregate>(string bucketId, Guid id) where TAggregate : class, IAggregate
        {
            return GetById<TAggregate>(id);
        }

        public TAggregate GetById<TAggregate>(string bucketId, Guid id, int version) where TAggregate : class, IAggregate
        {
            return GetById<TAggregate>(id);
        }

        public void Save(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
        {
            AggregatesToGet[aggregate.Id] = aggregate;
            
            SavedAggregate = aggregate;            
        }

        public void Save(string bucketId, IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
        {
            AggregatesToGet[aggregate.Id] = aggregate;

            SavedAggregate = aggregate;            
        }

        public RepositoryMock MockGetById(Guid id, object aggregate)
        {
            AggregatesToGet.Add(id, aggregate);

            return this;
        }

        public RepositoryMock MockGetById<T>(Guid id) where  T: AggregateBase
        {
            AggregateFactory factory = new AggregateFactory();
            AggregatesToGet.Add(id, factory.Build(typeof(T), id, null, null));

            return this;
        }

        public void Dispose()
        {
            
        }
    }
}
