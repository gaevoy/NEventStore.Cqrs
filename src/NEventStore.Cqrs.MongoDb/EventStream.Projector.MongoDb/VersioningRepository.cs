using System.Linq;
using EventStream.Projector;
using EventStream.Projector.Persistence;
using MongoDB.Driver;

namespace NEventStore.Cqrs.MongoDb.EventStream.Projector.MongoDb
{
    public class ProjectionInfoStore : IProjectionInfoStore
    {
        private readonly MongoCollection<ProjectionVersionDto> collection;
        public ProjectionInfoStore(string connectionString)
        {
            collection = GetDatabase(connectionString).GetCollection<ProjectionVersionDto>("_Projections");
        }

        public void Save(params ProjectionInfo[] projection)
        {
            foreach (var info in projection)
                collection.Save(new ProjectionVersionDto
                {
                    Id = GetName(info.Projection),
                    IsExist = info.IsExist,
                    Version = info.Version
                });
        }

        public ProjectionInfo[] Restore(params IProjection[] projections)
        {
            return (from projection in projections
                    let dto = collection.FindOneById(GetName(projection))
                    select new ProjectionInfo(projection, dto == null ? null : dto.Version, dto != null && dto.IsExist)).ToArray();
        }

        private static MongoDatabase GetDatabase(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetServer().GetDatabase(new MongoUrl(connectionString).DatabaseName);
            return database;
        }

        private static string GetName(IProjection projection)
        {
            return projection.GetType().FullName;
        }
    }
}
