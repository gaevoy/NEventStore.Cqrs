using System.Reflection;

namespace NEventStore.Cqrs.MongoDb
{
    public static class CqrsMongoWireupExtensions
    {
        public static CqrsMongoWireup WithMongo(this CqrsWireup wireup, string connectionName, params Assembly[] assemblies)
        {
            return new CqrsMongoWireup(wireup, connectionName, assemblies);
        }
    }
}
