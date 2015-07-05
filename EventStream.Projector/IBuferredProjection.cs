namespace EventStream.Projector
{
    public interface IBuferredProjection
    {
        void Begin();
        void Flush();
    }
}
