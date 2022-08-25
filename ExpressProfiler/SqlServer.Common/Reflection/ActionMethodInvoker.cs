using System;
using System.Collections.Generic;
using SqlServer.Logging;

namespace SqlServer.Reflection
{
    public class ActionMethodInvoker
    {
        private readonly Delegate _action;
        private readonly LogMethod _logMethod;

        private object[] _args;
        public Dictionary<string, object> LogValues { get; private set; }

        public ActionMethodInvoker(Delegate action)
        {
            _action = action;
            _logMethod = LogMethod.GetLogMethod(action.Method);
        }

        public string Name
        {
            get { return _logMethod.DisplayName; }
        }

        private void InitLogValues()
        {
            LogValues = new Dictionary<string, object>(_logMethod.LogParams.Length);
            foreach (var logParam in _logMethod.LogParams)
            {
                var value = _args[logParam.Index];
                var logValue = logParam.GetLogValue(value);

                LogValues.Add(logParam.Name, logValue);
            }
        }

        public void InitArgs(Func<int, object> argsReader)
        {
            _args = new object[_logMethod.Parameters.Length];

            for (var index = 0; index < _args.Length; index++)
            {
                _args[index] = argsReader(index);
            }

            InitLogValues();
        }

        public void InitParams(params object[] args)
        {
            if (args == null)
            {
                args = new object[0];
            }

            if (args.Length != _logMethod.Parameters.Length)
            {
                throw new InvalidOperationException("Bad args.");
            }

            _args = args;

            InitLogValues();
        }
        

        public object Execute()
        {
            return _action.DynamicInvoke(_args);
        }

        public T Execute<T>()
        {
            return (T) Execute();
        }
    }
}
