using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace EventStream.Projector
{
    public static class ProjectionHasher
    {
        public static string StructureHash(this IProjection projection, params Type[] dtoTypes)
        {
            var hashSource = new StringBuilder(100);
            AddTypeHash(projection.GetType(), hashSource);

            foreach (var type in dtoTypes.OrderBy(x => x.Name).ToList())
                AddTypeHash(type, hashSource);

            return ToMD5(hashSource.ToString());
        }

        private static void AddTypeHash(Type type, StringBuilder hashSource)
        {
            hashSource.Append(type.Name);

            var properties = type.GetProperties().OrderBy(x => x.Name);
            foreach (var propertyInfo in properties)
            {
                hashSource.Append(propertyInfo.Name);

                hashSource.Append(
                    propertyInfo.PropertyType.IsGenericType
                        ? propertyInfo.PropertyType.ToString().Replace(propertyInfo.PropertyType.Namespace, string.Empty)
                        : propertyInfo.PropertyType.Name);

                var attributes = propertyInfo.GetCustomAttributes(false).OrderBy(x => x.GetType().Name);
                foreach (var attribute in attributes)
                {
                    hashSource.Append(attribute.GetType().Name);
                }
            }

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(e => e.Name == "Handle");
            foreach (var method in methods)
            {
                var parameters = method.GetParameters().OrderBy(x => x.Name);
                foreach (var parameter in parameters)
                {
                    hashSource.Append(method.Name);
                    AddTypeHash(parameter.ParameterType, hashSource);
                }
            }
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
