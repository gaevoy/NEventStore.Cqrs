using System;
using System.Data;
using NEventStore.Cqrs.Projections;
using ServiceStack.OrmLite;

namespace NEventStore.Cqrs.MsSql.Projections
{
    public class CheckpointStore : ICheckpointStore
    {
        public const string REGULAR = "regular";
        public const string PROJECTION_CHANGE = "projection_change";

        private readonly OrmLiteConnectionFactory db;
        public CheckpointStore(string connectionString)
        {
            db = new OrmLiteConnectionFactory(connectionString, SqlServerDialect.Provider);
            db.DialectProvider.UseUnicode = true;
            db.DialectProvider.DefaultStringLength = 4000;
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
                using (IDbCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "UPDATE ProjectionCheckpointDto SET CommitIdProcessed = @CommitId, CommitStampProcessed = @CommitStamp WHERE Mode = @Mode";
                    AddParam(cmd, "@CommitId", DbType.Guid, checkpoint.CommitIdProcessed);
                    AddParam(cmd, "@CommitStamp", DbType.DateTime, checkpoint.CommitStampProcessed);
                    AddParam(cmd, "@Mode", DbType.String, checkpoint.Mode);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void AddParam(IDbCommand cmd, string name, DbType type, object val)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.DbType = type;
            param.Value = val ?? DBNull.Value;
            cmd.Parameters.Add(param);
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
