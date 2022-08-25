using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace SqlServer
{
    using Helpers;

    /// <summary>
    /// Represent single logic action.
    /// Id format desc: https://github.com/dotnet/runtime/blob/master/src/libraries/System.Diagnostics.DiagnosticSource/src/ActivityUserGuide.md#id-format
    /// </summary>
    public class ExpressActivity : IDisposable
    {
        private static long _actionCounter;
        private const int MaxSize = 200;
        private const char BeginChar = '|';
        private const char NameDelimiterChar = '-';
        private const char DelimiterChar = '.';
        private const char OverflowChar = '#';

        [ThreadStatic]
        private static ExpressActivity _current;

        private readonly Stopwatch _watch;

        private ExpressActivity(string operationName, ExpressActivity parent, IList<KeyValuePair<string,string>> baggage = null)
        {
            OperationName = operationName;
            Parent = parent;
            if (Parent == null)
            {
                Id = GenerateRootActivityId();
            }
            else
            {
                if (Parent.Id.Length < MaxSize)
                {
                    Id = GenerateChildActivityId(Parent.Id);
                }
                else
                {
                    Id = GenerateOverflowActivityId(Parent.Id);
                }
            }

            InitBaggage(baggage);

            _watch = Stopwatch.StartNew();
        }

        private ExpressActivity(string operationName, ExpressActivity parent, string parentForeignId, IList<KeyValuePair<string, string>> baggage = null)
        {
            OperationName = operationName;
            Parent = parent;
            if (parentForeignId.Length < MaxSize)
            {
                Id = GenerateChildForeignActivityId(parentForeignId);
            }
            else
            {
                Id = GenerateOverflowActivityId(parentForeignId);
            }

            InitBaggage(baggage);
            _watch = Stopwatch.StartNew();
        }

        public string OperationName { get; private set; }
        public ExpressActivity Parent { get; private set; }
        public string Id { get; private set; }

        public IList<KeyValuePair<string, string>> Baggage { get; private set; }

        public TimeSpan Elapsed
        {
            get { return _watch.Elapsed; }
        }

        public long ElapsedMs
        {
            get { return _watch.ElapsedMilliseconds; }
        }

        private void InitBaggage(IList<KeyValuePair<string, string>> baggage)
        {
            if (baggage == null)
            {
                Baggage = Parent != null ? Parent.Baggage : null;
                if (Baggage == null)
                {
                    Baggage = ArrayHelper.Empty<KeyValuePair<string, string>>();
                }
            }
            else
            {
                if (Parent == null)
                {
                    Baggage = baggage;
                }
                else
                {
                    var dic = new Dictionary<string, string>();
                    dic.OverrideValues(Parent.Baggage);
                    dic.OverrideValues(baggage);

                    Baggage = dic.ToArray();
                }
            }
        }

        public ExpressActivity GetRootParent()
        {
            var parent = this;
            while (parent.Parent != null)
            {
                parent = parent.Parent;
            }

            return parent;
        }

        public static ExpressActivity Current
        {
            get
            {
                return _current;
            }
        }

        public void Dispose()
        {
            if (_current == this)
            {
                _current = this.Parent;
            }
        }

        public static string GenerateRootActivityId()
        {
            var next = GetNextActionNumber();

            return string.Concat(BeginChar, ExpressApp.InstanceId, NameDelimiterChar, next, DelimiterChar);
        }

        public static string GenerateChildActivityId(string parentActionId)
        {
            var next = GetNextActionNumber();

            var builder = new StringBuilder(parentActionId);
            if (builder.Length > 0 && builder[builder.Length - 1] != DelimiterChar)
            {
                builder.Append(DelimiterChar);
            }

            builder.Append(next).Append(DelimiterChar);

            return builder.ToString();
        }

        public static string GenerateChildForeignActivityId(string parentForeignActionId)
        {
            var next = GetNextActionNumber();

            var builder = new StringBuilder();
            if (parentForeignActionId[0] != BeginChar)
            {
                builder.Append(BeginChar);
            }
            builder.Append(parentForeignActionId);

            if (builder[builder.Length - 1] != DelimiterChar)
            {
                builder.Append(DelimiterChar);
            }

            builder.Append(ExpressApp.InstanceId).Append(NameDelimiterChar).Append(next);
            builder.Append(DelimiterChar);
            return builder.ToString();
        }

        public static string GenerateOverflowActivityId(string parentActionId)
        {
            var builder = new StringBuilder();
            if (parentActionId[0] != BeginChar)
            {
                builder.Append(BeginChar);
            }
            builder.Append(parentActionId);

            if (builder[builder.Length - 1] == DelimiterChar)
            {
                builder.Remove(builder.Length - 1, 1);
            }

            builder.Append(OverflowChar);

            return builder.ToString();
        }

        public static long GetNextActionNumber()
        {
            var next = Interlocked.Increment(ref _actionCounter);

            return next;
        }

        public static ExpressActivity Create(string operationName, IList<KeyValuePair<string, string>> baggage = null)
        {
            var operation = new ExpressActivity(operationName, _current, baggage);
            _current = operation;

            return operation;
        }

        public static ExpressActivity Create(string operationName, string foreignActionId, IList<KeyValuePair<string, string>> baggage = null)
        {
            var operation = new ExpressActivity(operationName, _current, foreignActionId, baggage);
            _current = operation;

            return operation;
        }

        public static void Execute(string operationName, Action action)
        {
            using (Create(operationName))
            {
                action();
            }
        }

        public static T Execute<T>(string operationName, Func<T> action)
        {
            using (Create(operationName))
            {
                return action();
            }
        }
    }
}
