using System;
using NLog;

namespace Wickes.Logging
{
    internal class NLogLogger : ILogger
    {
        private readonly Logger _logger;
        private readonly NLogOptions _options;
        private readonly NLogBeginScopeParser _beginScopeParser;
        internal const string OriginalFormatPropertyName = "{OriginalFormat}";

        public NLogLogger(Logger logger, NLogOptions options, NLogBeginScopeParser beginScopeParser)
        {
            _logger = logger;
            _options = options ?? NLogOptions.Default;
            _beginScopeParser = beginScopeParser;
        }

        public void Log(Type type, LogEventInfo eventInfo)
        {
            _logger.Log(type, eventInfo);
        }

        public string Name { get { return _logger.Name; } }
        public NLogOptions Options { get { return _options; } }

        /// <summary>
        /// Is logging enabled for this logger at this <paramref name="logLevel"/>?
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _logger.Name;
        }

        /// <summary>
        /// Begin a scope. Use in config with ${ndlc}
        /// </summary>
        /// <param name="state">The state (message)</param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            if (!_options.IncludeScopes || state == null)
            {
                return NullScope.Instance;
            }

            try
            {
                return _beginScopeParser.ParseBeginScope(state) ?? NullScope.Instance;
            }
            catch (Exception ex)
            {
                NLog.Common.InternalLogger.Debug(ex, "Exception in BeginScope");
                return NullScope.Instance;
            }
        }

        private sealed class NullScope : IDisposable
        {
            static NullScope()
            {
                Instance = new NullScope();
            }

            public static NullScope Instance { get; private set; }

            private NullScope()
            {
            }

            /// <inheritdoc />
            public void Dispose()
            {
                // Nothing to do
            }
        }
    }
}
