using System.Collections.Generic;

namespace EventStream.Projector
{
    public interface IEventStream
    {
        IEnumerable<EventsSlice> Read(Checkpoint? fromCheckpoint);
    }
}
