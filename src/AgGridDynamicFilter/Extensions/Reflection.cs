using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AgGridDynamicFilter
{
    public static partial class Extensions
    {
        private static object locker = new object();
       
        private static Dictionary<Type, List<PropertyInfo>> ObjectProperties = new Dictionary<Type, List<PropertyInfo>>();

        public static IEnumerable<PropertyInfo> GetPublicPropertiesFromCache(this Type type)
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();

            if (!ObjectProperties.TryGetValue(type,out properties) )
            {
                properties = type.GetPublicProperties();
                lock(locker)
                {
                    if (!ObjectProperties.ContainsKey(type))
                        ObjectProperties.Add(type, properties);
                }
            }

            return properties;
        }

        public static PropertyInfo GetPublicPropertyFromCache(this Type type, string propertyName)
        {
            return type.GetPublicPropertiesFromCache().SingleOrDefault(e=> string.Equals(e.Name, propertyName, StringComparison.OrdinalIgnoreCase));
        }

        public static List<PropertyInfo> GetPublicProperties(this Type type)
        {
            if (type.IsInterface)
            {
                var propertyInfos = new List<PropertyInfo>();

                var considered = new List<Type>();
                var queue = new Queue<Type>();
                considered.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    var subType = queue.Dequeue();
                    foreach (var subInterface in subType.GetInterfaces())
                    {
                        if (considered.Contains(subInterface))
                            continue;

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

                    var typeProperties = subType.GetProperties(
                        BindingFlags.FlattenHierarchy
                        | BindingFlags.Public
                        | BindingFlags.Instance);

                    var newPropertyInfos = typeProperties
                        .Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToList();
            }

            return type.GetProperties(BindingFlags.FlattenHierarchy
                | BindingFlags.Public | BindingFlags.Instance).ToList();
        }

        public static T ChangeType<T>(this object value)
        {
            var t = typeof(T);

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return default;
                }

                t = Nullable.GetUnderlyingType(t);
            }

            return (T)Convert.ChangeType(value, t);
        }

        public static object ChangeType(object value, Type conversion)
        {
            var t = conversion;

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                t = Nullable.GetUnderlyingType(t);
            }

            return Convert.ChangeType(value, t);
        }
    }
}
