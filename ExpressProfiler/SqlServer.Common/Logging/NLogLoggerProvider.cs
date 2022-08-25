using System;
using NLog;

namespace Express.Logging
{
    public class NLogLoggerProvider //: ILoggerProvider
    {
        private readonly NLogBeginScopeParser _beginScopeParser;

        /// <summary>
        /// NLog options
        /// </summary>
        public NLogOptions Options { get; set; }

        /// <summary>
        /// NLog Factory
        /// </summary>
        public LogFactory LogFactory { get; private set; }

        /// <summary>
        /// New provider with default options, see <see cref="Options"/>
        /// </summary>
        public NLogLoggerProvider()
            : this(NLogOptions.Default)
        {
        }

        /// <summary>
        /// New provider with options
        /// </summary>
        /// <param name="options"></param>
        public NLogLoggerProvider(NLogOptions options)
            : this(options, LogManager.LogFactory)
        {
        }

        /// <summary>
        /// New provider with options
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logFactory">Optional isolated NLog LogFactory</param>
        public NLogLoggerProvider(NLogOptions options, LogFactory logFactory)
        {
            LogFactory = logFactory;
            Options = options;
            _beginScopeParser = new NLogBeginScopeParser(options);
        }

        /// <summary>
        /// Create a logger with the name <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of the logger to be created.</param>
        /// <returns>New Logger</returns>
        public ILogger CreateLogger(string name)
        {
            return new NLogLogger(LogFactory.GetLogger(name), Options, _beginScopeParser);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Options.ShutdownOnDispose)
                {
                    LogFactory.Shutdown();
                }
                else
                {
                    LogFactory.Flush();
                }
            }
        }

    }
}
