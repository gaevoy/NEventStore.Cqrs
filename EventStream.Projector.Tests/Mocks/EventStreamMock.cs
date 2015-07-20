using System.Collections.Generic;

namespace EventStream.Projector.Tests.Mocks
{
    public class LocalEventStream : IEventStream
    {
        private readonly IEnumerable<EventsSlice> stream;

        public LocalEventStream(IEnumerable<EventsSlice> stream)
        {
            this.stream = stream;
        }

        public IEnumerable<EventsSlice> Read(Checkpoint? fromCheckpoint)
        {
            return stream;
            /*bool found = false;
            foreach (var eventsSlice in stream)
            {
                if (fromCheckpoint == null)
                    yield return eventsSlice;
                else if (eventsSlice.Checkpoint.Position == fromCheckpoint.Value.Position)
                    found = true;
                else if (found)
                    yield return eventsSlice;
            }*/
        }
    }
}
