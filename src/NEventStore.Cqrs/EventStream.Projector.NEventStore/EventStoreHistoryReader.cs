using System.Collections.Generic;
using System.Linq;
using EventStream.Projector;

namespace NEventStore.Cqrs.EventStream.Projector.NEventStore
{
    public class NEventStoreStream : global::EventStream.Projector.IEventStream
    {
        private readonly IStoreEvents es;

        public NEventStoreStream(IStoreEvents es)
        {
            this.es = es;
        }

        public IEnumerable<EventsSlice> Read(Checkpoint? fromCheckpoint)
        {
            return es.Advanced.GetFrom(fromCheckpoint == null ? null : fromCheckpoint.Value.Position)
                .Where(e => !e.Headers.ContainsKey("SagaType"))
                .Select(Map);
        }

        EventsSlice Map(ICommit commit)
        {
            return new EventsSlice(new Checkpoint(commit.CheckpointToken), commit.Events.Select(e => e.Body));
        }
    }
}
