namespace EventStream.Projector
{
    public struct Checkpoint
    {
        public const string Default = "Default";
        public const string ProjectionChange = "ProjectionChange"; 

        public readonly string Position;

        public Checkpoint(string position)
        {
            Position = position;
        }
    }
}