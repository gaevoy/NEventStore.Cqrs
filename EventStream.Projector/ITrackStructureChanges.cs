using System;

namespace EventStream.Projector
{
    public interface ITrackStructureChanges
    {
        Type[] TrackTypes { get; }
    }
}
