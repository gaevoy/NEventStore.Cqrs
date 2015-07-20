namespace EventStream.Projector.Persistence
{
    public interface ICheckpointStore
    {
        void Save(Checkpoint? position, string scope);
        Checkpoint? Restore(string scope);
    }
}
