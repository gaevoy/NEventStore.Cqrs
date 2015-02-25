namespace NEventStore.Cqrs.MsSql
{
    public static class CqrsMsSqlWireupExtensions
    {
        public static CqrsMsSqlWireup WithMsSql(this CqrsWireup wireup, string connectionName, string readModelsConnectionName)
        {
            return new CqrsMsSqlWireup(wireup, connectionName, readModelsConnectionName);
        }
    }
}
