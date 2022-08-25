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
                AppLogger.Initialize(SqlServerLogger.versionString);
                Logger = Logging.AppLogger.CreateLogger<SqlServerLogger>();
                using (var client = new SqlServerLogger())
                {
                    client.StartProfiling();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Environment.ExitCode = 1;
                return;
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //MessageBox.Show(((Exception)e.ExceptionObject).Message , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

    }
}
