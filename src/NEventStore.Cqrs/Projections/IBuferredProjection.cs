namespace NEventStore.Cqrs.Projections
{
    public interface IBuferredProjection
    {
        void Begin();
        void Flush();
    }
}
