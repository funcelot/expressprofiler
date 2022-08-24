using Wickes.Logging;

namespace Wickes.Configuration
{
    public abstract class WickesAppBuilderBase : IWickesApp
    {
        private static readonly ILogWriter Logger = InternalLogger.Create<WickesAppBuilderBase>(new DiagnosticsLogWriterFactory());

        protected WickesAppBuilderBase(string appName)
        {
            AppName = appName;
        }

        public string AppName { get; private set; }

        protected virtual NLogVariablesResolver CreateNLogVariablesResolver()
        {
            return new NLogVariablesResolver(AppName);
        }

        protected virtual IAppLoggerBuilder CreateLoggerBuilder()
        {
            return new NLogByXmlAppLoggerBuilder(CreateNLogVariablesResolver());
        }

        protected void LogAppStartedSafe(ILogger logger)
        {
            logger.LogAppStartedSafe(AppName);
        }

        protected virtual void LogAppStarted(ILogger logger)
        {
            logger.LogAppStarted(AppName);
        }

        public virtual ILogger Initialize()
        {
            Logger.Write("Starting app");
            var logBuilder = CreateLoggerBuilder();
            ILogger logger = AppLogger.EnsureInited(logBuilder);

            LogAppStartedSafe(logger);

            logger.LogInformation("Started app '{AppName}'", AppName);
            return logger;
        }
    }
}
