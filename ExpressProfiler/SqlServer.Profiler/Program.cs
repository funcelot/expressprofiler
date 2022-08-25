using System;
using SqlServer.Logging;

namespace SqlServer.Logger
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ILogger Logger;
            try
            {
                AppLogger.Initialize("SqlServer.Logger");
                Logger = Logging.AppLogger.CreateLogger<SqlServerLogger>();
                Logger.LogInformation("Logging started");
                using (var client = new SqlServerLogger())
                {
                    client.StartProfiling();
                }
                Logger.LogInformation("Logging ended");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Environment.ExitCode = 1;
                return;
            }
        }
    }
}
