using System.Collections.Generic;
using System.Diagnostics;

namespace EventStream.Projector
{
    [DebuggerDisplay("EventsSlice (Checkpoint = {Checkpoint.Position})")]
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