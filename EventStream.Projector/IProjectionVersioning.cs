namespace EventStream.Projector
{
    public interface IProjectionVersioning
    {
        IProjection[] SelectModified(params IProjection[] projections);
        string GetModifiedReason(IProjection projection);
        void MarkAsUnmodified(params IProjection[] projection);
    }
}
