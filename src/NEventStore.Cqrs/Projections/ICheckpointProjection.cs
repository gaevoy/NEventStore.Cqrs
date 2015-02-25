namespace NEventStore.Cqrs.Projections
{
    public interface ICheckpointStore
    {
        void EnsureInitialized();
        Checkpoint Load(string mode);
        void Save(Checkpoint checkpoint);
    }
}
