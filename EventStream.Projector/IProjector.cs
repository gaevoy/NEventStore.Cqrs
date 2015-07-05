using System.Collections.Generic;

namespace EventStream.Projector
{
    public interface IProjector
    {
        void Handle(EventsSlice evt);
        void HandleAll(IEnumerable<EventsSlice> evts);
    }
}
