using System.Linq;
using EventStream.Projector;
using EventStream.Projector.Persistence;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace NEventStore.Cqrs.MongoDb.EventStream.Projector.MongoDb
{
    public class CheckpointStore : ICheckpointStore
    {
        private readonly MongoCollection<ProjectionCheckpointDto> collection;
        public CheckpointStore(string connectionString)
        {
            collection = GetDatabase(connectionString).GetCollection<ProjectionCheckpointDto>("_Checkpoints");
        }

        public void Save(Checkpoint? checkpoint, string scope)
        {
            collection.Save(new ProjectionCheckpointDto { Id = scope, Position = checkpoint == null ? null : checkpoint.Value.Position });
        }

        public Checkpoint? Restore(string scope)
        {
            var dto = collection.AsQueryable().FirstOrDefault(e => e.Id == scope);
            return dto == null || dto.Position == null ? (Checkpoint?)null : new Checkpoint(dto.Position);
        }

        private static MongoDatabase GetDatabase(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetServer().GetDatabase(new MongoUrl(connectionString).DatabaseName);
            return database;
        }
    }
}
