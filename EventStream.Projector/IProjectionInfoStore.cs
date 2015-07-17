namespace EventStream.Projector
{
    public interface IProjectionInfoStore
    {
        void Save(params ProjectionInfo[] projection);
        ProjectionInfo[] Restore(params string[] projections);
    }
}
