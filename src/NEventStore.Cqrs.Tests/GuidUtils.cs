using System;
using System.Collections.Generic;
using System.Linq;

namespace NEventStore.Cqrs.Tests
{
    public static class GuidUtils
    {
        public static Guid? ToGuid(int? id)
        {
            return (id == null) ? (Guid?)null : ToGuid(id.Value);
        }

        public static Guid ToGuid(int id)
        {
            return new Guid("00000000-0000-0000-0000-" + id.ToString("000000000000"));
        }

        public static int ToInt(Guid id)
        {
            var bytes = id.ToByteArray();
            return bytes[bytes.Length - 1];
        }

        public static List<Guid> ToGuidList(string list)
        {
            return string.IsNullOrWhiteSpace(list) ? new List<Guid>() : list.Split(',').Select(int.Parse).Select(ToGuid).ToList();
        }

        public static Guid? Guid(this int? id)
        {
            return ToGuid(id);
        }

        public static Guid Guid(this int id)
        {
            return ToGuid(id);
        }

        public static int Int(this Guid id)
        {
            return ToInt(id);
        }
    }
}
