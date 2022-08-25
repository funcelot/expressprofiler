using SqlServer.Configuration;

namespace SqlServer.Logging
{
    public class DefaultAppBuilder : AppBuilderBase
    {
        public DefaultAppBuilder(string appName)
            : base(appName)
        {
        }

        protected override NLogVariablesResolver CreateNLogVariablesResolver()
        {
            return new DefaultNLogVariablesResolverByAppSettings(AppName);
        }

        protected override IAppLoggerBuilder CreateLoggerBuilder()
        {
            return new DefaultNLogByXmlAppLoggerBuilder(CreateNLogVariablesResolver());
        }
    }

}
