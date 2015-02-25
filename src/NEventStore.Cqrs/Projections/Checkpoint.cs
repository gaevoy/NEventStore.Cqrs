using System;

namespace NEventStore.Cqrs.Projections
{
    public class Checkpoint
    {
        public Checkpoint(string mode, ICommit commit)
        {
            Mode = mode;
            Set(commit);
        }
        public Checkpoint(string mode, Guid? commitIdProcessed, DateTime? commitStampProcessed)
        {
            Mode = mode;
            CommitIdProcessed = commitIdProcessed;
            CommitStampProcessed = commitStampProcessed;
        }
        public Checkpoint(string mode)
            : this(mode, null)
        {

        }

        public const string REGULAR = "regular";
        public const string PROJECTION_CHANGE = "projection_change";
        public string Mode { get; private set; }
        public Guid? CommitIdProcessed { get; private set; }
        public DateTime? CommitStampProcessed { get; private set; }

        public bool IsUndefined
        {
            get
            {
                return (CommitIdProcessed.HasValue == false || CommitStampProcessed.HasValue == false);
            }
        }

        public bool IsProcessed(ICommit commit)
        {
            ValidateCommitStamp(commit);
            return (CommitIdProcessed == commit.CommitId);
        }

        private void ValidateCommitStamp(ICommit commit)
        {
            if (commit.CommitStamp.AddSeconds(-2) > CommitStampProcessed)
            {
                throw new Exception(string.Format("Commit {0} {1} marked with checkpoint can not be found", CommitIdProcessed, CommitStampProcessed));
            }
        }

        public void Set(ICommit commit)
        {
            if (commit == null)
            {
                CommitIdProcessed = null;
                CommitStampProcessed = null;
            }
            else
            {
                CommitIdProcessed = commit.CommitId;
                CommitStampProcessed = commit.CommitStamp;
            }
        }

        public override string ToString()
        {
            return CommitIdProcessed.ToString();
        }
    }
}
