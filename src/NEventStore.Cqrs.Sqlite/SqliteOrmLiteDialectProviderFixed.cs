using System;
using ServiceStack.OrmLite.Sqlite;

namespace NEventStore.Cqrs.Sqlite
{
    public class SqliteOrmLiteDialectProviderFixed : SqliteOrmLiteDialectProvider
    {
        static SqliteOrmLiteDialectProviderFixed()
        {
            SqliteOrmLiteDialectProvider.Instance = new SqliteOrmLiteDialectProviderFixed();
        }

        private const string DATE_STRING_ENDING = "Z'";

        public override object ConvertDbValue(object value, Type type)
        {
            object val = base.ConvertDbValue(value, type);
            if (type == typeof(DateTime) && val != null)
            {
                val = ((DateTime)val).ToUniversalTime();
            }
            return val;
        }

        public override string GetQuotedValue(object value, Type fieldType)
        {
            string quotedValue = base.GetQuotedValue(value, fieldType);
            if (fieldType == typeof(DateTime) && value != null && !quotedValue.EndsWith(DATE_STRING_ENDING))
            {
                quotedValue = quotedValue.Substring(0, quotedValue.Length - 1) + DATE_STRING_ENDING;
            }
            return quotedValue;
        }
    }
}
