using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using NEventStore.Cqrs.Projections;

namespace NEventStore.Cqrs.MongoDb.Projections
{
    public class CheckpointStore : ICheckpointStore
    {
        public const string REGULAR = "regular";
        public const string PROJECTION_CHANGE = "projection_change";

        private readonly MongoCollection<ProjectionCheckpointDto> collection;
        public CheckpointStore(string connectionString)
            : this(GetDatabase(connectionString))
        {

        }

        internal CheckpointStore(MongoDatabase database)
        {
            collection = database.GetCollection<ProjectionCheckpointDto>("ProjectionCheckpoints");
        }

        public void EnsureInitialized()
        {
            EnsureInitialized(REGULAR);
        }

        public Checkpoint Load(string mode)
        {
            EnsureInitialized(mode);
            var dto = collection.AsQueryable().SingleOrDefault(e => e.Id == mode);
            if (dto != null)
            {
                return new Checkpoint(dto.Id, dto.CommitIdProcessed, dto.CommitStampProcessed);
            }
            return null;
        }

        public void Save(Checkpoint checkpoint)
        {
            collection.Update(
                Query<ProjectionCheckpointDto>
                    .EQ(c => c.Id, checkpoint.Mode),
                Update<ProjectionCheckpointDto>
                    .Set(c => c.CommitIdProcessed, checkpoint.CommitIdProcessed)
                    .Set(c => c.CommitStampProcessed, checkpoint.CommitStampProcessed));
        }

        private void EnsureInitialized(string mode)
        {
            var checkpoint = collection.FindOneById(new BsonString(mode));
            if (checkpoint == null)
            {
                checkpoint = new ProjectionCheckpointDto { Id = mode };
                collection.Insert(checkpoint);
            }
        }

        private static MongoDatabase GetDatabase(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetServer().GetDatabase(new MongoUrl(connectionString).DatabaseName);
            return database;
        }
    }
}
