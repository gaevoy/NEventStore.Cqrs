using System.Collections.Concurrent;
using EventStream.Projector.Persistence;

namespace EventStream.Projector.Tests.Mocks
{
    public class CheckpointStoreMock : ICheckpointStore
    {
        readonly ConcurrentDictionary<string, Checkpoint?> db = new ConcurrentDictionary<string, Checkpoint?>();

        public void Save(Checkpoint? position, string scope)
        {
            db[scope] = position;
        }

        public Checkpoint? Restore(string scope)
        {
            Checkpoint? result;
            return db.TryGetValue(scope, out result) ? result : null;
        }
    }
}