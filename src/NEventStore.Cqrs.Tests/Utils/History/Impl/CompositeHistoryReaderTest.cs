using System;
using System.Linq;
using NEventStore.Cqrs.EventStream.Projector.NEventStore;
using NEventStore.Cqrs.Tests.Mocks;
using NEventStore.Persistence;
using NUnit.Framework;

namespace NEventStore.Cqrs.Tests.Utils.History.Impl
{
    [TestFixture]
    public class CompositeHistoryReaderTest
    {
        [Test]
        public void Read()
        {
            // Given
            var r1 = new HistoryReaderMock().MockRead(new[] { NewCommit(2), NewCommit(3), NewCommit(3) });
            var r2 = new HistoryReaderMock().MockRead(new[] { NewCommit(1), NewCommit(4), NewCommit(10), NewCommit(15) });
            var r3 = new HistoryReaderMock().MockRead(new Commit[] { });
            var r4 = new HistoryReaderMock().MockRead(new [] { NewCommit(2), NewCommit(11) });

            // When
            var reader = new CompositeHistoryReader(r1, r2, r3, r4);

            // Then
            var actual = reader.Read(new DateTime(1, 1, 1), new DateTime(1, 1, 1)).Select(e => e.CommitStamp.Year);
            CollectionAssert.AreEqual(actual, new[] { 1, 2, 2, 3, 3, 4, 10, 11, 15 });
        }

        private Commit NewCommit(int year)
        {
            return new Commit("", "", 0, Guid.Empty, 0, new DateTime(year, 1, 1), "", null, null);
        }
    }
}
