using NEventStore.Persistence.Sql.SqlDialects;

namespace NEventStore.Cqrs.MsSql
{
    public class MsSqlDialectFixes : MsSqlDialect
    {
        public override string InitializeStorage
        {
            get { return base.InitializeStorage.Replace("[tinyint]", "[int]"); }
        }
    }
}
