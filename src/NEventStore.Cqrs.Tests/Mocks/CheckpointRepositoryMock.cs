using System.Collections.Generic;
using NEventStore.Cqrs.Projections;

namespace NEventStore.Cqrs.Tests.Mocks
{
    public class CheckpointStoreMock : ICheckpointStore
    {
        readonly Dictionary<string, Checkpoint> checkpoints = new Dictionary<string, Checkpoint>();

        public void EnsureInitialized()
        {
            
        }

        public Checkpoint Load(string mode)
        {
            return checkpoints.ContainsKey(mode)
                ? checkpoints[mode]
                : new Checkpoint(mode);
        }

        public void Save(Checkpoint checkpoint)
        {
            checkpoints[checkpoint.Mode] = checkpoint;
        }

        public CheckpointStoreMock MockLoad(string mode, Checkpoint result)
        {
            checkpoints[mode] = result;
            return this;
        }
    }
}