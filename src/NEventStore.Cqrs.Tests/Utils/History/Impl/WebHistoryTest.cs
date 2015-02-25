using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using NEventStore.Cqrs.Impl.Utils.History;
using NEventStore.Cqrs.Messages;
using NEventStore.Cqrs.Tests.Mocks;
using NEventStore.Persistence;
using NUnit.Framework;

namespace NEventStore.Cqrs.Tests.Utils.History.Impl
{
    [TestFixture]
    public class WebHistoryTest
    {
        [TestCase(/*start:*/1, /*end:*/5, /*page:*/6, "1,2,3,4,5,6,7,8,9,10,11,12,13,14,15")]
        [TestCase(/*start:*/1, /*end:*/5, /*page:*/100, "1,2,3,4,5,6,7,8,9,10,11,12,13,14,15")]
        [TestCase(/*start:*/1, /*end:*/5, /*page:*/5, "1,2,3,4,5,6,7,8,9,10,11,12,13,14,15")]
        [TestCase(/*start:*/3, /*end:*/5, /*page:*/6, "9,10,11,12,13,14,15")]
        [TestCase(/*start:*/1, /*end:*/4, /*page:*/6, "1,2,3,4,5,6,7,8,9,10,11,12")]
        public void Interop(int start, int end, int page, string expected)
        {
            // Given
            var advanced = new PersistStreamsMock().MockGetFromTo(GetFromTo);
            var es = new StoreEventsMock().MockAdvanced(advanced);
            var endpoint = new WebHistoryEndpoint(es);
            var downloader = new DownloaderMock().MockDownloadString(url => DownloadString(endpoint, url));
            var reader = new WebHistoryReader("http://localhost:456/Api/History", downloader, page);

            // When
            var actual = reader.Read(NewDateTime(start), NewDateTime(end));

            // Then
            CollectionAssert.AreEqual(ToGuidList(expected), actual.Select(e => e.CommitId).ToArray());
        }

        [Test]
        public void EventDeserialization()
        {
            // Given
            var advanced = new PersistStreamsMock()
                .MockGetFromTo((start, end) => GetFromToWithEvent(start, end, new SomethingHappenedEvent { Data = 123 }));
            var es = new StoreEventsMock().MockAdvanced(advanced);
            var endpoint = new WebHistoryEndpoint(es);
            var downloader = new DownloaderMock().MockDownloadString(url => DownloadString(endpoint, url));
            var reader = new WebHistoryReader("http://localhost:456/Api/History", downloader, 100);

            // When
            var actual = reader.Read(NewDateTime(1), NewDateTime(2)).First();

            // Then
            Assert.That(actual.Events.Count, Is.EqualTo(1));
            Assert.That(actual.Events.First().Body, Is.TypeOf<SomethingHappenedEvent>());
            Assert.That(((SomethingHappenedEvent)actual.Events.First().Body).Data, Is.EqualTo(123));
        }

        private IEnumerable<ICommit> GetFromTo(DateTime start, DateTime end)
        {
            var commits = new[]
            {
                NewCommit(id: 1, year: 1),
                NewCommit(id: 2, year: 1),
                NewCommit(id: 3, year: 1),
                NewCommit(id: 4, year: 1),
                NewCommit(id: 5, year: 2),
                NewCommit(id: 6, year: 2),
                NewCommit(id: 7, year: 2),
                NewCommit(id: 8, year: 2),
                NewCommit(id: 9, year: 3),
                NewCommit(id: 10, year: 3),
                NewCommit(id: 11, year: 3),
                NewCommit(id: 12, year: 3),
                NewCommit(id: 13, year: 4),
                NewCommit(id: 14, year: 4),
                NewCommit(id: 15, year: 4)
            };
            return commits.Where(e => e.CommitStamp >= start && e.CommitStamp < end); //https://github.com/NEventStore/NEventStore/blob/b6f34442d17a30e22a2b492c41063afbbf770c4e/src/NEventStore/Persistence/Sql/SqlDialects/CommonSqlStatements.resx#L155
        }

        private IEnumerable<ICommit> GetFromToWithEvent(DateTime start, DateTime end, object evt)
        {
            var commit = new Commit("", "", 0, ToGuid(1), 0, start, "", null, new[] { new EventMessage { Body = evt } });
            return new ICommit[] { commit };
        }

        private string DownloadString(WebHistoryEndpoint endpoint, string url)
        {
            NameValueCollection query = ParseQueryString(url);
            return endpoint.Read(query["start"], query["end"], query["pageSize"]);
        }

        private Commit NewCommit(int id, int year)
        {
            return new Commit("", "", 0, ToGuid(id), 0, NewDateTime(year), "", null, null);
        }

        private DateTime NewDateTime(int year)
        {
            return new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        public Guid ToGuid(int id)
        {
            return GuidUtils.ToGuid(id);
        }
        public List<Guid> ToGuidList(string list)
        {
            return string.IsNullOrWhiteSpace(list) ? new List<Guid>() : list.Split(',').Select(int.Parse).Select(ToGuid).ToList();
        }

        public static NameValueCollection ParseQueryString(string s)
        {
            NameValueCollection nvc = new NameValueCollection();

            // remove anything other than query string from url
            if (s.Contains("?"))
            {
                s = s.Substring(s.IndexOf('?') + 1);
            }

            foreach (string vp in Regex.Split(s, "&"))
            {
                string[] singlePair = Regex.Split(vp, "=");
                if (singlePair.Length == 2)
                {
                    nvc.Add(singlePair[0], singlePair[1]);
                }
                else
                {
                    // only one key with no value specified in query string
                    nvc.Add(singlePair[0], string.Empty);
                }
            }

            return nvc;
        }

        private class SomethingHappenedEvent : DomainEvent
        {
            public int Data { get; set; }
        }
    }
}
