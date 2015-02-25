namespace NEventStore.Cqrs.Sqlite
{
    public static class CqrsSqliteWireupExtensions
    {
        public static CqrsSqliteWireup WithSqlite(this CqrsWireup wireup, string connectionName, string readModelsConnectionName)
        {
            return new CqrsSqliteWireup(wireup, connectionName, readModelsConnectionName);
        }
    }
}
