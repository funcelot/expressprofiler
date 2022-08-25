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
            AppLogger.Initialize("SqlServer.Logger");
            ILogger Logger = Logging.AppLogger.CreateLogger<SqlServerLogger>();
            try
            {
                Logger.LogInformation("Logging started");
                using (var client = new SqlServerLogger())
                {
                    client.StartProfiling();
                    while (client.IsProfiling())
                    {
                        client.Process();
                    }
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
