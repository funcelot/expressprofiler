using NLog;
using System;

namespace SqlServer.Logging
{
    public class NLogAppLoggerBuilder : IAppLoggerBuilder
    {
        private static readonly ILogWriter _logWriter;

        static NLogAppLoggerBuilder()
        {
            _logWriter = InternalLogger.Create<NLogAppLoggerBuilder>(new DiagnosticsLogWriterFactory());
        }

        private Guid _id = Guid.NewGuid();

        public Guid Id
        {
            get
            {
                return _id;
            }
        }

        protected virtual NLogOptions CreateNLogOptions()
        {
            return new NLogOptions();
        }

        protected virtual LogFactory CreateLogFactory(NLogOptions options)
        {
            var logFactory = new LogFactory();
            logFactory.ThrowExceptions = options.ThrowExceptions != null ? options.ThrowExceptions.Value : logFactory.ThrowExceptions;
            logFactory.ThrowConfigExceptions = options.ThrowConfigExceptions != null ? options.ThrowConfigExceptions.Value : logFactory.ThrowConfigExceptions;
            logFactory.KeepVariablesOnReload = options.KeepVariablesOnReload != null ? options.KeepVariablesOnReload.Value : logFactory.KeepVariablesOnReload;
            logFactory.AutoShutdown = options.AutoShutdown != null ? options.AutoShutdown.Value : logFactory.AutoShutdown;
            logFactory.GlobalThreshold = options.GlobalThreshold != null ? options.GlobalThreshold : logFactory.GlobalThreshold;
            return logFactory;
        }

        public ILoggerFactory CreateLoggerFactory()
        {
            try
            {
                _id = Guid.NewGuid();

                _logWriter.Write("Creating NLogLoggerFactory");

                NLogGlobalSettings.EnsureInited();

                var options = CreateNLogOptions();
                var logFactory = CreateLogFactory(options);
                var factory = new NLogLoggerFactory(options, logFactory);

                _logWriter.Write("Created NLogLoggerFactory");

                return factory;
            }
            catch (Exception ex)
            {
                _logWriter.Write("Failed create NLogLoggerFactory: {0}", ex);
                throw;
            }
        }
    }
}
