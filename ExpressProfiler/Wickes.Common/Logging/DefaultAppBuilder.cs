using Wickes.Configuration;

namespace Wickes.Logging
{
    public class DefaultAppBuilder : WickesAppBuilderBase
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
