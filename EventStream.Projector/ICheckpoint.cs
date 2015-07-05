namespace EventStream.Projector
{
    public interface ICheckpoint
    {
        void Save(string position, CheckpointScope scope);
        string Restore(CheckpointScope scope);
    }
}
