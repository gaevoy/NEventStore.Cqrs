using System;
using System.Data.SqlClient;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NEventStore.Cqrs.Projections;

namespace NEventStore.Cqrs.MongoDb.Projections
{
    internal class VersioningRepository : IVersioningRepository
    {
        private readonly MongoCollection<ProjectionVersionDto> collection;
        public VersioningRepository(string connectionString)
            : this(GetDatabase(connectionString))
        {

        }

        internal VersioningRepository(MongoDatabase database)
        {
            collection = database.GetCollection<ProjectionVersionDto>("ProjectionVersions");
        }

        public bool IsModified(IProjection projection)
        {
            return GetModifiedReason(projection) != string.Empty;
        }

        public string GetModifiedReason(IProjection projection)
        {
            if (projection == null || projection is ICheckpointStore)
            {
                return string.Empty;
            }
            try
            {
                var dto = collection.AsQueryable().FirstOrDefault(x => x.ProjectionName == GetName(projection));

                if (dto == null)
                {
                    return "Version is absent";
                }
                if (dto.Version != projection.Version)
                {
                    return "Version is different";
                }
                if (dto.Hash != StructureHash.CalculateMD5(projection))
                {
                    return "Hash is different";
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                if (ex is SqlException || ex.Message.Contains("Invalid column name"))
                {
                    return "Invalid column name";
                }

                throw;
            }
        }

        public bool IsVersionExist(IProjection projection)
        {
            return collection.AsQueryable().Any(x => x.ProjectionName == GetName(projection));
        }

        public IProjection[] SelectModified(params IProjection[] projections)
        {
            if (projections == null ||
                !projections.Any())
            {
                return new IProjection[] { };
            }

            return projections
                .Where(IsModified)
                .ToArray();
        }

        public bool IsInitialized(IProjection[] projections)
        {
            return projections.Any(IsVersionExist);
        }

        public void MarkAsUnmodified(IProjection projection)
        {
            string name = GetName(projection);

            var dto = collection.AsQueryable().FirstOrDefault(x => x.ProjectionName == GetName(projection)) ?? new ProjectionVersionDto();
            dto.ProjectionName = name;
            dto.Version = projection.Version;
            dto.ChangeDate = DateTime.UtcNow;
            dto.Hash = StructureHash.CalculateMD5(projection);

            collection.Save(dto);
        }

        private static string GetName(IProjection projection)
        {
            return projection.GetType().FullName;
        }

        private static MongoDatabase GetDatabase(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetServer().GetDatabase(new MongoUrl(connectionString).DatabaseName);
            return database;
        }
    }
}
