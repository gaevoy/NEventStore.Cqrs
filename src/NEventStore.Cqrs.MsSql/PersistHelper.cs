using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using NEventStore.Cqrs.Utils;

namespace NEventStore.Cqrs.MsSql
{
    public class PersistHelper : IPersistHelper
    {
        private readonly string connectionString;

        public PersistHelper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<Guid> GetIdsOfAggregates()
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();

                var cmd = con.CreateCommand();
                cmd.CommandText = @" Select [StreamId] from (Select [StreamId], CAST(headers as varchar(max)) as header from [dbo].[Commits]) t where t.header not like '%SagaType%' Group by [StreamId] ";

                var ids = new List<Guid>();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ids.Add(reader.GetGuid(0));
                    }
                }

                return ids;
            }
        }
        public void ClearSnapshots()
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = @"DELETE FROM [dbo].[Snapshots]";
                cmd.ExecuteNonQuery();
            }
        }
    }
}
