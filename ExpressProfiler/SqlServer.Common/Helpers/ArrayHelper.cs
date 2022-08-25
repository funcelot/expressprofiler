using System.Collections.Generic;

namespace SqlServer.Helpers
{
    public static class ArrayHelper
    {
        internal static class EmptyArray<T>
        {
            public static readonly T[] Value = new T[0];
        }

        public static T[] Empty<T>()
        {
            return EmptyArray<T>.Value;
        }

        public static void Append<TKey, TValue>(this IList<KeyValuePair<TKey, TValue>> list, TKey key, TValue value)
        {
            list.Add(new KeyValuePair<TKey, TValue>(key, value));
        }
    }
}
