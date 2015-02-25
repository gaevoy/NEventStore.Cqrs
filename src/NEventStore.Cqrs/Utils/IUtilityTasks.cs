using System.Threading;

namespace NEventStore.Cqrs.Utils
{
    public interface IUtilityTasks
    {
        void RebuildProjections(CancellationToken? pause = null, bool fromTheStart = false);
        void PrintChangedProjections();
        void RebuildSnapshots(int maxEventsThreshold = 500);
        void ClearSnapshots();
        void CheckAggregatesReplay();
    }
}
