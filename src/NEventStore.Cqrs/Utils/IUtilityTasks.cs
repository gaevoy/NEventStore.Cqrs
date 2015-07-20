namespace NEventStore.Cqrs.Utils
{
    public interface IUtilityTasks
    {
        void RebuildSnapshots(int maxEventsThreshold = 500);
        void ClearSnapshots();
        void CheckAggregatesReplay();
    }
}
