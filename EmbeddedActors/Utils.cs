using System;
using System.Collections.Generic;
using System.Reflection;

namespace EmbeddedActors
{
    public static class Utils
    {
        public static IEnumerable<TypeInfo> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.DefinedTypes;
            }
            catch (ReflectionTypeLoadException)
            {
                return null;
            }
        }

        public static void ForEach<K, V>(this Dictionary<K, V> dict, Action<K, V, int> action)
        {
            int i = 0;

            foreach (KeyValuePair<K, V> kvp in dict)
            {
                action(kvp.Key, kvp.Value, i++);
            }
        }

        public static void ForEach<K, V>(this Dictionary<K, V> dict, Action<K, V> action)
        {
            dict.ForEach((k, v, i) => action(k, v));
        }

        public static void ForEach<K>(this IEnumerable<K> enumeration, Action<K, int> action)
        {
            int i = 0;

            foreach (K item in enumeration)
            {
                action(item, i++);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            enumeration.ForEach((s, i) => action(s));
        }

        public static TResult Find<TKey, TResult>(this IDictionary<TKey, TResult> dictionary, TKey key) where TResult : class
        {
            TResult result;
            return !dictionary.TryGetValue(key, out result) ? null : result;
        }
    }
}
