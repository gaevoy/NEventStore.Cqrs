using System;
using System.Collections.Generic;
using System.Linq;
using NEventStore.Cqrs.Utils.History;
using NEventStore.Persistence;

namespace NEventStore.Cqrs.Impl.Utils.History
{
    public class EventStoreHistoryReader : IHistoryReader
    {
        private readonly IStoreEvents es;

        public EventStoreHistoryReader(IStoreEvents es)
        {
            this.es = es;
        }

        public IEnumerable<Commit> Read(DateTime start, DateTime end)
        {
            return es.Advanced.GetFromTo(start, end).Cast<Commit>();
        }
    }
}
