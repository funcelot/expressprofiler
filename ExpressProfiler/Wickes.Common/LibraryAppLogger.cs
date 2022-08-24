using System;

namespace Wickes.Logging
{
    public class LibraryAppLogger
    {
        public static ILogger Initialize(string appLoggerName)
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
    }
}
