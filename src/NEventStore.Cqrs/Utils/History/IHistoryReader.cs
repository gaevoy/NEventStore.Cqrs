using System;
using System.Collections.Generic;
using NEventStore.Persistence;

namespace NEventStore.Cqrs.Utils.History
{
    public interface IHistoryReader
    {
        IEnumerable<Commit> Read(DateTime start, DateTime end);
    }
}
