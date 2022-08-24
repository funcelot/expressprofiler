using System;
using Wickes.Configuration;
using Wickes.Logging;

namespace Wickes
{
    public class WickesAppLogger
    {
        public static ILogger Initialize(string appLoggerName = "WickesAppLogger")
        {
            try
            {
                return WickesApp.EnsureStarted(() => new DefaultAppBuilder(appLoggerName));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Environment.ExitCode = 1;
            }
            return null;
        }

        public static ILogger InitializeLibrary(string appLoggerName = "WickesAppLogger")
        {
            return Initialize(appLoggerName);
        }

        public static ILogger Initialize<T>(string appLoggerName = "WickesAppLogger") where T : IWickesApp
        {
            try
            {
                return WickesApp.EnsureStarted(() => (T)Activator.CreateInstance(typeof(T), appLoggerName));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Environment.ExitCode = 1;
            }
            return null;
        }
    }
}
