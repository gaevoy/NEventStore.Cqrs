using System.Threading;

namespace EventStream.Projector
{
    public interface IProjectionRebuild
    {
        void Start(IEventStream eventStream, CancellationToken running);
        string GetChangedProjectionsInfo(bool showEmptyInfo);
        bool IsRequired();
    }
}
