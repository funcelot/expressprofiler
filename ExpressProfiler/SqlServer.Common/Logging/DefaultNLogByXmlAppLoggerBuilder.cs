using SqlServer.Helpers;

namespace SqlServer.Logging
{
    public class DefaultNLogByXmlAppLoggerBuilder : NLogByXmlAppLoggerBuilder
    {
        public DefaultNLogByXmlAppLoggerBuilder(INLogVariablesResolver resolver)
            : base(resolver)
        {
        }

        public DefaultNLogByXmlAppLoggerBuilder(string appName)
            : this(new NLogVariablesResolver(appName))
        {
        }

        protected override string LoadXmlConfig(NLogOptions options)
        {
            string resourceFileContent = DirectoryHelper.GetResourcesFileContent(AppSettingsHelper.GetSettingsOrDefault("NLog.FileName", options.NLogFileName));
            if (resourceFileContent != null)
            {
                return resourceFileContent;
            }
            else
            {
                return base.LoadXmlConfig(options);
            }
        }
    }
}
