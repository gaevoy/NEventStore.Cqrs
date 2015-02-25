using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NEventStore.Cqrs.Messages;

namespace NEventStore.Cqrs.Projections
{
    public class StructureHash
    {
        public static string CalculateMD5(IProjection projection)
        {
            var typeSource = projection as ITrackStructureChanges;
            var dtoTypes = (typeSource == null) ? new Type[0] : typeSource.TrackTypes;
            var hashSource = new StringBuilder(100);
            AddTypeHash(projection.GetType(), hashSource);

            hashSource.Append(dtoTypes.Length);
            foreach (var type in dtoTypes.OrderBy(x => x.Name).ToList())
                AddTypeHash(type, hashSource);

            return ToMD5(hashSource.ToString());
        }

        private static void AddTypeHash(Type type, StringBuilder hashSource)
        {
            hashSource.Append(type.Name);

            var properties = type.GetProperties().OrderBy(x => x.Name);
            hashSource.Append(properties.Count());

            foreach (var propertyInfo in properties)
            {
                hashSource.Append(propertyInfo.Name);

                hashSource.Append(
                    propertyInfo.PropertyType.IsGenericType
                        ? propertyInfo.PropertyType.ToString().Replace(propertyInfo.PropertyType.Namespace, string.Empty)
                        : propertyInfo.PropertyType.Name);

                var attributes = propertyInfo.GetCustomAttributes(false).OrderBy(x => x.GetType().Name);
                hashSource.Append(attributes.Count());
                foreach (var attribute in attributes)
                {
                    hashSource.Append(attribute.GetType().Name);
                }
            }

            var interfaces = type.GetInterfaces().OrderBy(x => x.FullName);
            hashSource.Append(interfaces.Count());

            foreach (var @interface in interfaces)
            {
                if (@interface.IsGenericType)
                {
                    var arguments = @interface.GetGenericArguments().OrderBy(x => x.Name);
                    foreach (var argument in arguments)
                    {
                        hashSource.Append(argument.Name);
                        if (argument.IsSubclassOf(typeof(IEvent)))
                        {
                            AddTypeHash(argument, hashSource);
                        }
                    }
                }
            }

            var methods = type.GetMethods();
            hashSource.Append(methods.Count());
        }

        private static string ToMD5(string value)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] retVal = md5.ComputeHash(Encoding.Unicode.GetBytes(value));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
