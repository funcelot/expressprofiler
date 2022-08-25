using System;
using System.Runtime.InteropServices;
using System.Threading;
using SqlServer.Logging;

namespace SqlServer.Logger
{
    static class Program
    {
        public static int SleepInterval = 250;

        #region Trap application termination

        public static volatile bool Exit = false;

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            //allow main to run off
            Exit = true;

            return true;
        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                AppLogger.Initialize("SqlServer.Logger");
                ILogger Logger = Logging.AppLogger.CreateLogger<SqlServerLogger>();

                // Some boilerplate to react to close window event, CTRL-C, kill, etc
                _handler += new EventHandler(Handler);
                SetConsoleCtrlHandler(_handler, true);

                Logger.LogInformation("Logging started");
                using (var client = new SqlServerLogger())
                {
                    client.StartProfiling();
                    while (!Exit)
                    {
                        client.Wait();
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
