using SqlServer.Logging;

namespace SqlServer.Configuration
{
    public abstract class AppBuilderBase : IApp
    {
        private static readonly ILogWriter Logger = InternalLogger.Create<AppBuilderBase>(new DiagnosticsLogWriterFactory());

        protected AppBuilderBase(string appName)
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
            ILogger logger = Logging.AppLogger.EnsureInited(logBuilder);

            LogAppStartedSafe(logger);

            logger.LogInformation("Started app '{AppName}'", AppName);
            return logger;
        }
    }
}
