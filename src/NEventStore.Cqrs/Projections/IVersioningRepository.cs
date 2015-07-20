namespace NEventStore.Cqrs.Projections
{
    public interface IVersioningRepository
    {
        IProjection[] SelectModified(params IProjection[] projections);
        bool IsInitialized(IProjection[] projections);
        string GetModifiedReason(IProjection projection);
        void MarkAsUnmodified(IProjection projection);
    }
}