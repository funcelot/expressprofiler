using System.IO;
using SqlServer.Common;

namespace SqlServer.Logging
{
    public class DefaultNLogVariablesResolverByAppSettings : NLogVariablesResolver
    {
        public DefaultNLogVariablesResolverByAppSettings(string appName)
            : base(appName)
        {
        }

        public override string Resolve(string name)
        {
            string value = ReadSetting("logging." + name);
            if (name == "w-logFolder")
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var directoryInfo = new DirectoryInfo(value);
                    return directoryInfo.FullName;
                }
                else
                {
                    string directoryName = Path.Combine(LoggerConfiguration.BaseLoggingPath, AppName);
                    var directoryInfo = new DirectoryInfo(directoryName);
                    return directoryInfo.FullName;
                }
            }
            if (name == "w-appName")
            {
                if (value == null)
                {
                    return base.Resolve(name);
                }
                if (string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }
            return base.Resolve(name);
        }
    }
}
