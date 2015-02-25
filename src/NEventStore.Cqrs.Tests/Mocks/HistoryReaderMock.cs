using System;
using System.Collections.Generic;
using NEventStore.Cqrs.Utils.History;
using NEventStore.Persistence;

namespace NEventStore.Cqrs.Tests.Mocks
{
    public class HistoryReaderMock : IHistoryReader
    {
        private IEnumerable<Commit> readResult;

        public IEnumerable<Commit> Read(DateTime start, DateTime end)
        {
            return readResult;
        }

        public HistoryReaderMock MockRead(IEnumerable<Commit> result)
        {
            readResult = result;
            return this;
        }
    }
}
