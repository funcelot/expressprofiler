using System;
using System.Collections.Generic;

namespace Express.Helpers
{
    public static class DictionaryHelper
    {
        public static bool TryGetValueWithLock<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TValue value)
        {
            lock (dictionary)
            {
                return dictionary.TryGetValue(key, out value);
            }
        }

        public static TValue GetOrAddWithLock<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> getValue)
        {
            lock (dictionary)
            {
                TValue value;
                if (!dictionary.TryGetValue(key, out value))
                {
                    value = getValue(key);
                    dictionary.Add(key, value);
                }

                return value;
            }
        }

        public static TValue GetWithCheck<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }

            throw new Exception(string.Format("Can't find key '{0}'.", key));
        }

        public static IDictionary<TKey, TValue> OverrideValues<TKey, TValue>(this IDictionary<TKey, TValue> dic, IEnumerable<KeyValuePair<TKey, TValue>> overrideValues)
        {
            foreach (var pair in overrideValues)
            {
                dic[pair.Key] = pair.Value;
            }

            return dic;
        }

        public static IDictionary<TKey, TValue> MergeValues<TKey, TValue>(this IDictionary<TKey, TValue> baseValue, IDictionary<TKey, TValue> overrideValues)
        {
            var result = new Dictionary<TKey, TValue>(baseValue);
            result.OverrideValues(overrideValues);

            return result;
        }
    }
}
