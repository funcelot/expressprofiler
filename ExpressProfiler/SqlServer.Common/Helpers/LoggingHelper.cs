using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SqlServer.Helpers
{
    public static class LoggingHelper
    {
        public static string ToLogString<T>(this T value)
            where T : class
        {
            if (value == null)
            {
                return "NULL";
            }

            var result = value.ToString();
            if (string.IsNullOrEmpty(result))
            {
                return "EMPTY";
            }

            return result;
        }

        public static string JoinAll(this IEnumerable values, string delimiter = ", ")
        {
            var builder = new StringBuilder();
            var first = true;
            foreach (var value in values)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(delimiter);
                }

                builder.Append(value);
            }

            return builder.ToString();
        }

        public static string ToLogStringAsKeyValueList<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> keyValueList)
        {
            var builder = new StringBuilder();
            foreach (var pair in keyValueList)
            {
                builder.Append(pair.Key).Append('=').Append(pair.Value).Append(", ");
            }

            if (builder.Length == 0)
            {
                return string.Empty;
            }

            return builder.ToString(0, builder.Length - 2);
        }

        public static string ToLogIdString(this object logId)
        {
            var byteLogId = logId as byte[];

            if (byteLogId == null)
                return "Null";

            if (byteLogId.Length != 8)
                return "0xlen" + byteLogId.Length;

            var strbyteLogId = BitConverter.ToString(byteLogId);
            string v = strbyteLogId != null ? strbyteLogId.Replace("-", string.Empty) : string.Empty;
            string stringLogId = "0x" + v;
            return stringLogId;
        }

        public static string ToLogString<T>(this IList<T> values)
        {
            if (values == null)
            {
                return null;
            }

            return values.JoinAll();
        }
    }
}
