using System;
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
            if (position == null) throw new ArgumentNullException("position");
            Position = position;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Checkpoint?;
            return other != null && Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }

        public override string ToString()
        {
            return Position;
        }

        public bool Equals(Checkpoint other)
        {
            return Position == other.Position;
        }

        public static bool operator ==(Checkpoint a, Checkpoint b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Checkpoint a, Checkpoint b)
        {
            return !a.Equals(b);
        }

        public static implicit operator string(Checkpoint checkpoint)
        {
            return checkpoint.Position;
        }

        public static implicit operator string(Checkpoint? checkpoint)
        {
            return checkpoint == null ? null : checkpoint.Value.Position;
        }

        public static implicit operator Checkpoint?(string position)
        {
            return position == null ? (Checkpoint?)null : new Checkpoint(position);
        }
    }
}