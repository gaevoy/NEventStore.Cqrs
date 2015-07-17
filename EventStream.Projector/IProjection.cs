namespace EventStream.Projector
{
    public interface IProjection
    {
        string Version { get; }
        void Clear();
    }
}
