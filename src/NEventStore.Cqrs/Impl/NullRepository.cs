using System;
using System.Collections.Generic;
using CommonDomain;
using CommonDomain.Persistence;

namespace NEventStore.Cqrs.Impl
{
    public class NullRepository : IRepository
    {
        public void Dispose()
        {
        }

        public TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate
        {
            return null;
        }

        public TAggregate GetById<TAggregate>(Guid id, int version) where TAggregate : class, IAggregate
        {
            return null;
        }

        public TAggregate GetById<TAggregate>(string bucketId, Guid id) where TAggregate : class, IAggregate
        {
            return null;
        }

        public TAggregate GetById<TAggregate>(string bucketId, Guid id, int version) where TAggregate : class, IAggregate
        {
            return null;
        }

        public void Save(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
        {
        }

        public void Save(string bucketId, IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
        {
        }
    }
}
