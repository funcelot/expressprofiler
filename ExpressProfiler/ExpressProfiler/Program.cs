using System;
using Express;
using Express.Logging;

namespace ExpressProfiler
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
                ExpressAppLogger.Initialize(ExpressProfiler.versionString);
                Logger = AppLogger.CreateLogger<ExpressProfiler>();
                using (var client = new ExpressProfiler())
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
