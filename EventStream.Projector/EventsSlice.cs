using System.Collections.Generic;

namespace EventStream.Projector
{
    public struct EventsSlice
    {
        public readonly Checkpoint Checkpoint;
        public readonly IEnumerable<object> Events;

        public EventsSlice(Checkpoint checkpoint, IEnumerable<object> events)
        {
            Checkpoint = checkpoint;
            Events = events;
        }
    }
}