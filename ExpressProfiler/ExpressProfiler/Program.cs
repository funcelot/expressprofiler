using System;
using System.Windows.Forms;
using Wickes;
using Wickes.Logging;

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
            ExpressProfiler client = null;
            try
            {
                WickesAppLogger.Initialize(ExpressProfiler.versionString);
                Logger = AppLogger.CreateLogger<ExpressProfiler>();
                client = new ExpressProfiler();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Environment.ExitCode = 1;
                return;
            }
            finally 
            {
                if (client != null)
                    client.StopProfiling();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //MessageBox.Show(((Exception)e.ExceptionObject).Message , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

    }
}
