using System.Collections.Generic;

namespace EventStream.Projector
{
    public interface IEventStream
    {
        IEnumerable<EventsSlice> Read(string checkpoint);
    }
}
