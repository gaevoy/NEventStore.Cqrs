namespace EventStream.Projector.Persistence
{
    public interface IProjectionInfoStore
    {
        void Save(params ProjectionInfo[] projection);
        ProjectionInfo[] Restore(params IProjection[] projections);
    }
}
