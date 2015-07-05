namespace EventStream.Projector
{
    public interface IProjection
    {
        int Version { get; }
        void Clear();
    }
}
