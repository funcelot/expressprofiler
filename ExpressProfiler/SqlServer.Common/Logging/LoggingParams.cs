using System.Collections.Generic;
using SqlServer.Helpers;

namespace SqlServer.Logging
{
    public class LoggingParams : List<KeyValuePair<string, object>>
    {
        public LoggingParams()
        {

        }

        public LoggingParams(IEnumerable<KeyValuePair<string, object>> values)
            : base(values)
        {

        }

        public override string ToString()
        {
            return this.ToLogStringAsKeyValueList();
        }

        public static LoggingParams Create(string name, object value)
        {
            var result = new LoggingParams();
            result.Append(name, value);

            return result;
        }
    }
}
