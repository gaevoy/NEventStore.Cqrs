namespace EventStream.Projector.Persistence
{
    public interface ICheckpointStore
    {
        void Save(Checkpoint? checkpoint, string scope);
        Checkpoint? Restore(string scope);
    }
}
