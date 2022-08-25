using System;

namespace SqlServer.Logging
{
    public class LibraryAppLogger
    {
        public static ILogger Initialize(string appLoggerName)
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
    }
}
