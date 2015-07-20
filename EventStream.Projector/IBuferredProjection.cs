namespace EventStream.Projector
{
    public interface IBuferredProjection : IProjector
    {
        void Begin();
        void Flush();
    }
}
