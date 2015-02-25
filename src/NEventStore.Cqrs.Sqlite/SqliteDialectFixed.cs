using System;
using NEventStore.Persistence.Sql.SqlDialects;

namespace NEventStore.Cqrs.Sqlite
{
    public class SqliteDialectFixed : SqliteDialect
    {
        public override DateTime ToDateTime(object value)
        {
            if (value is DateTime)
            {
                var dt = (DateTime)value;
                if (dt.Kind == DateTimeKind.Unspecified)
                {
                    dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, DateTimeKind.Utc);
                    return dt;
                }
            }
            return base.ToDateTime(value);
        }
    }
}
