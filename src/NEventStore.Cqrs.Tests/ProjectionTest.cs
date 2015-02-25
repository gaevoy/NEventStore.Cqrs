using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NEventStore.Cqrs.Messages;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NEventStore.Cqrs.Tests
{
    public abstract class ProjectionTest
    {
        protected virtual string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["ReadDb"].ConnectionString; }
        }

        protected Guid? ToGuid(int? id)
        {
            return GuidUtils.ToGuid(id);
        }

        protected Guid ToGuid(int id)
        {
            return GuidUtils.ToGuid(id);
        }

        protected DateTime ToDateTime(int year, int month = 1, int day = 1)
        {
            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        }

        protected int ToInt(Guid id)
        {
            return GuidUtils.ToInt(id);
        }

        protected List<Guid> ToGuidList(string list)
        {
            return string.IsNullOrWhiteSpace(list) ? new List<Guid>() : list.Split(',').Select(int.Parse).Select(ToGuid).ToList();
        }

        protected string ToString(IEnumerable<Guid> list)
        {
            return string.Join(",", list.Select(ToInt));
        }

        protected void AreAllPropertiesEqual(object expected, object actual)
        {
            Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(actual));
        }

        ConcurrentDictionary<Type, object> projections = new ConcurrentDictionary<Type, object>();
        protected T New<T>() where T : class, IProjection
        {
            var type = typeof(T);
            object projection;
            if (!projections.TryGetValue(type, out projection))
            {
                projection = Activator.CreateInstance(type, ConnectionString);
                projections[type] = projection;
            }
            var result = (T)projection;
            result.Clear();
            return result;
        }
    }
}
