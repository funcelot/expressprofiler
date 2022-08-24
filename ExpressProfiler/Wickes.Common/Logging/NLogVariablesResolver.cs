using System;
using System.Configuration;

namespace Wickes.Logging
{
    public class NLogVariablesResolver : INLogVariablesResolver
    {
        public NLogVariablesResolver(string appName)
        {
            AppName = appName;
        }

        public string AppName { get; private set; }

        public virtual string Resolve(string name)
        {
            if (string.Equals(name, "w-appName", StringComparison.OrdinalIgnoreCase))
            {
                return AppName;
            }

            return null;
        }

        public static string ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                return appSettings[key];
            }
            catch (ConfigurationErrorsException)
            {
                return null;
            }
        }
    }
}
