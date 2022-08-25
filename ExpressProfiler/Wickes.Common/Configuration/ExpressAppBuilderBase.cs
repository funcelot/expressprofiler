using Express.Logging;

namespace Express.Configuration
{
    public abstract class ExpressAppBuilderBase : IExpressApp
    {
        private static readonly ILogWriter Logger = InternalLogger.Create<ExpressAppBuilderBase>(new DiagnosticsLogWriterFactory());

        protected ExpressAppBuilderBase(string appName)
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
