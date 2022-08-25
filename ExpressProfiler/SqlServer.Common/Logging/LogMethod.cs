using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlServer.Logging
{
    public class LogMethod
    {
        private const string WrappedMethodNameEnding = "Impl";

        public LogMethod(MethodInfo method)
        {
            Method = method;
            Name = string.Format("{0}.{1}", method.DeclaringType.Name, method.Name);
            DisplayName = CreateDisplayName(Name);
            
            Parameters = method.GetParameters();

            var logParams = new List<LogParameter>();
            for(var index=0;index<Parameters.Length;index++)
            {
                var getLogValue = LogValue.GetGetLogValue(Parameters[index].ParameterType);
                if (getLogValue == null)
                {
                    continue;
                }

                var p = new LogParameter
                {
                    Index = index,
                    Name = Parameters[index].Name,
                    GetLogValue = getLogValue,
                };
                logParams.Add(p);
            }

            LogParams = logParams.ToArray();
        }

        private static string CreateDisplayName(string name)
        {
            if (name.Length > WrappedMethodNameEnding.Length && name.EndsWith(WrappedMethodNameEnding))
            {
                var newName = name.Substring(0, name.Length - WrappedMethodNameEnding.Length);
                if (newName.Last() != '.')
                {
                    return newName;
                }
            }

            return name;
        }

        public MethodInfo Method { get; private set; }
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public ParameterInfo[] Parameters { get; private set; }

        public LogParameter[] LogParams { get; private set; }

        public class LogParameter
        {
            public int Index { get; internal set; }
            public string Name { get; internal set; }
            public Func<object, object> GetLogValue { get; internal set; }
        }

        private static readonly Dictionary<MethodInfo, LogMethod> _logMethods = new Dictionary<MethodInfo, LogMethod>();

        internal static void Clear()
        {
            lock (_logMethods)
            {
                _logMethods.Clear();
            }
        }

        public static LogMethod GetLogMethod(MethodInfo method)
        {
            lock (_logMethods)
            {
                LogMethod logMethod;
                if (!_logMethods.TryGetValue(method, out logMethod))
                {
                    logMethod = new LogMethod(method);
                    _logMethods.Add(method, logMethod);
                }

                return logMethod;
            }
        }
    }
}
