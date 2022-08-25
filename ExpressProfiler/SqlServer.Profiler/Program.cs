using System;
using Express;
using Express.Logging;

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
                ExpressAppLogger.Initialize(SqlServerLogger.versionString);
                Logger = AppLogger.CreateLogger<SqlServerLogger>();
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
