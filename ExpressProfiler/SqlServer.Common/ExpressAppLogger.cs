using System;
using Express.Configuration;
using Express.Logging;

namespace Express
{
    public class ExpressAppLogger
    {
        public static ILogger Initialize(string appLoggerName = "ExpressAppLogger")
        {
            try
            {
                return ExpressApp.EnsureStarted(() => new DefaultAppBuilder(appLoggerName));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Environment.ExitCode = 1;
            }
            return null;
        }

        public static ILogger InitializeLibrary(string appLoggerName = "ExpressAppLogger")
        {
            return Initialize(appLoggerName);
        }

        public static ILogger Initialize<T>(string appLoggerName = "ExpressAppLogger") where T : IExpressApp
        {
            try
            {
                return ExpressApp.EnsureStarted(() => (T)Activator.CreateInstance(typeof(T), appLoggerName));
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
