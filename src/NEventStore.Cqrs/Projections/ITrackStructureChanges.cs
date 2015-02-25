using System;

namespace NEventStore.Cqrs.Projections
{
    public interface ITrackStructureChanges
    {
        Type[] TrackTypes { get; }
    }
}
