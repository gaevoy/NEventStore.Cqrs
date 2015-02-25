using System;
using System.Data.SqlClient;
using System.Linq;
using NEventStore.Cqrs.Projections;
using ServiceStack.OrmLite;

namespace NEventStore.Cqrs.MsSql.Projections
{
    internal class VersioningRepository : IVersioningRepository
    {
        private readonly OrmLiteConnectionFactory db;

        public VersioningRepository(string connectionString)
        {
            db = new OrmLiteConnectionFactory(connectionString, SqlServerDialect.Provider);
            db.DialectProvider.UseUnicode = true;
            db.DialectProvider.DefaultStringLength = 4000;
            EnsureTableCreated();
        }
        internal VersioningRepository(OrmLiteConnectionFactory db)
        {
            this.db = db;
            EnsureTableCreated();
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
            using (var connection = db.OpenDbConnection())
            {
                try
                {
                    var dto = connection.FirstOrDefault<ProjectionVersionDto>(x => x.ProjectionName == GetName(projection));

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
        }

        public void EnsureTableCreated()
        {
            using (var con = db.OpenDbConnection())
            {
                con.CreateTableIfNotExists<ProjectionVersionDto>();
            }
        }

        public bool IsVersionExist(IProjection projection)
        {
            using (var con = db.OpenDbConnection())
            {
                return con.Count<ProjectionVersionDto>(x => x.ProjectionName == GetName(projection)) > 0;
            }
        }

        public IProjection[] SelectModified(params IProjection[] projections)
        {
            if (projections == null ||
                !projections.Any())
            {
                return new IProjection[] { };
            }

            if (!IsVersioningTableExists())
            {
                return projections;
            }

            return projections
                .Where(IsModified)
                .ToArray();
        }

        public bool IsInitialized(IProjection[] projections)
        {
            if (IsVersioningTableExists())
            {
                return projections.Any(IsVersionExist);
            }
            return false;
        }

        public void MarkAsUnmodified(IProjection projection)
        {
            string name = GetName(projection);

            using (var con = db.OpenDbConnection())
            {
                var dto = con.FirstOrDefault<ProjectionVersionDto>(x => x.ProjectionName == name) ?? new ProjectionVersionDto();

                dto.ProjectionName = name;
                dto.Version = projection.Version;
                dto.ChangeDate = DateTime.UtcNow;
                dto.Hash = StructureHash.CalculateMD5(projection);

                con.Save(dto);
            }
        }

        private bool IsVersioningTableExists()
        {
            return db.Run(conn => conn.TableExists("ProjectionVersionDto"));
        }

        private static string GetName(IProjection projection)
        {
            return projection.GetType().FullName;
        }
    }
}
