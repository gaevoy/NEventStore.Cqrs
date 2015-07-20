using System;
using System.Collections.Generic;
using System.Linq;
using EventStream.Projector;

namespace NEventStore.Cqrs.EventStream.Projector.NEventStore.Legacy
{
    public class NEventStoreStream : global::EventStream.Projector.IEventStream
    {
        private readonly IStoreEvents es;

        public NEventStoreStream(IStoreEvents es)
        {
            this.es = es;
        }

        public IEnumerable<EventsSlice> Read(Checkpoint? from)
        {
            DateTime commitStamp = DateTime.MinValue;
            Guid commitId = Guid.Empty;

            if (from != null)
                Parse(from.Value, out commitStamp, out commitId);

            var commits = es.Advanced.GetFrom(Bucket.Default, commitStamp).Where(e => !e.Headers.ContainsKey("SagaType"));
            bool startFound = (from == null);
            foreach (var commit in commits)
            {
                if (!startFound)
                {
                    if (commit.CommitId == commitId)
                    {
                        startFound = true;
                    }
                    continue;
                }
                yield return Map(commit);
            }
            if (!startFound)
                throw new Exception(string.Format("Checkpoint {0} can not be found. At the end", from));
        }

        EventsSlice Map(ICommit commit)
        {
            return new EventsSlice(CommitToCheckpoint(commit), commit.Events.Select(e => e.Body));
        }

        void Parse(Checkpoint checkpoint, out DateTime commitStamp, out Guid commitId)
        {
            string[] parts = checkpoint.Position.Split(' ');
            commitStamp = DateTime.Parse(parts[0]).ToUniversalTime();
            commitId = new Guid(parts[1]);
        }

        public static Checkpoint CommitToCheckpoint(ICommit commit)
        {
            return new Checkpoint(string.Concat(commit.CommitStamp.ToString("O"), " ", commit.CommitId));
        }
    }
}
