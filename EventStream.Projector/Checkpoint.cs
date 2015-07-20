using System.Diagnostics;

namespace EventStream.Projector
{
    [DebuggerDisplay("Checkpoint (Position = {Position})")]
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