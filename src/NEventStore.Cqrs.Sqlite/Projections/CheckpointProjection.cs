using System;
using System.Data;
using NEventStore.Cqrs.Projections;
using ServiceStack.OrmLite;

namespace NEventStore.Cqrs.Sqlite.Projections
{
    public class CheckpointStore : ICheckpointStore
    {
        public const string REGULAR = "regular";
        public const string PROJECTION_CHANGE = "projection_change";

        private readonly OrmLiteConnectionFactory db;
        public CheckpointStore(string connectionString)
        {
            db = new OrmLiteConnectionFactory(connectionString, new SqliteOrmLiteDialectProviderFixed());
        }
        internal CheckpointStore(OrmLiteConnectionFactory db)
        {
            this.db = db;
        }

        public void EnsureInitialized()
        {
            EnsureInitialized(REGULAR);
        }

        public Checkpoint Load(string mode)
        {
            EnsureInitialized(mode);
            using (IDbConnection con = db.OpenDbConnection())
            {
                var dto = con.IdOrDefault<ProjectionCheckpointDto>(mode);
                if (dto != null)
                {
                    return new Checkpoint(dto.Mode, dto.CommitIdProcessed, dto.CommitStampProcessed);
                }
                return null;
            }
        }

        public void Save(Checkpoint checkpoint)
        {
            using (var con = db.OpenDbConnection())
            {
                con.Update(new ProjectionCheckpointDto
                    {
                        Mode = checkpoint.Mode,
                        CommitIdProcessed = checkpoint.CommitIdProcessed,
                        CommitStampProcessed = checkpoint.CommitStampProcessed
                    });
            }
        }

        private void EnsureInitialized(string mode)
        {
            using (IDbConnection con = db.OpenDbConnection())
            {
                con.CreateTableIfNotExists<ProjectionCheckpointDto>();
                var checkpoint = con.IdOrDefault<ProjectionCheckpointDto>(mode);
                if (checkpoint == null)
                {
                    checkpoint = new ProjectionCheckpointDto { Mode = mode };
                    con.Insert(checkpoint);
                }
            }
        }
    }
}
