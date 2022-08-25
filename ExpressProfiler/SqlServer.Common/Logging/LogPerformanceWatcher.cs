using System.Diagnostics;

namespace Express.Logging
{
    internal static class LogPerformanceWatcher
    {
        private static readonly long TicksPerMs = Stopwatch.Frequency / 1000 ;

        private static int _count = 0;
        private static long _total = 0;

        private const int ReportEvery = 100;

        private static readonly object _locker = new object();

        private static ILogger Logger;

        public static void Init(ILoggerFactory factory)
        {
            Logger = AppLogger.CreateLogger(factory, typeof(LogPerformanceWatcher));
        }

        public static void ReportLogged(long ticks)
        {
            int count;
            long total;
            var report = false;
            lock (_locker)
            {
                count = ++_count;
                total = _total = _total + ticks;
                
                if (count % ReportEvery == 0)
                {
                    _count = 0;
                    _total = 0;
                    report = true;
                }
            }

            if (report)
            {
                var avgMs = total * 1.0 / count / TicksPerMs;
                Logger.LogDebug("Processed next {Total} log events for App '{AppId}'. Avg performance: {Avg} ms.", count, ExpressApp.InstanceId, avgMs.ToString("F3"));
            }
        }
    }
}
