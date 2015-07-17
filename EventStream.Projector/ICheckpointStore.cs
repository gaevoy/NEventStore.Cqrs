namespace EventStream.Projector
{
    public interface ICheckpointStore
    {
        void Save(Checkpoint? position, CheckpointScope scope);
        Checkpoint? Restore(CheckpointScope scope);
    }
}
