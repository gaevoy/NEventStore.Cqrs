namespace NEventStore.Cqrs
{
    public interface IProjection
    {
        int Version { get; }
        void Clear();
    }
}
