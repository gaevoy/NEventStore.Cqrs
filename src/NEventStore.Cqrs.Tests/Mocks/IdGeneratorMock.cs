using System;
using System.Collections.Generic;

namespace NEventStore.Cqrs.Tests.Mocks
{
    public class IdGeneratorMock : IIdGenerator
    {
        Queue<Guid> results;
        Guid result;
        public Guid NewGuid()
        {
            if (results != null) return results.Dequeue();

            return result;
        }

        public IdGeneratorMock MockNewGuid(Guid result)
        {
            this.result = result;

            return this;
        }

        public IdGeneratorMock MockNewGuidQueued(params Guid[] results)
        {
            this.results = new Queue<Guid>();
            foreach (var guid in results)
            {
                this.results.Enqueue(guid);
            }

            return this;
        }
    }
}