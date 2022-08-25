using Express.Configuration;

namespace Express.Logging
{
    public class DefaultAppBuilder : ExpressAppBuilderBase
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
