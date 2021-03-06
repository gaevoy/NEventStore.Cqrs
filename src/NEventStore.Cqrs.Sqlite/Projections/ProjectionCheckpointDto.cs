﻿using System;
using ServiceStack.DataAnnotations;

namespace NEventStore.Cqrs.Sqlite.Projections
{
    public class ProjectionCheckpointDto
    {
        [PrimaryKey]
        public string Mode { get; set; }
        public Guid? CommitIdProcessed { get; set; }
        public DateTime? CommitStampProcessed { get; set; }

        public bool IsEmpty()
        {
            return (CommitIdProcessed.HasValue == false || CommitStampProcessed.HasValue == false);
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
    }
}
