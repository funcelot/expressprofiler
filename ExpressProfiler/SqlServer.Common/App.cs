using System;
using System.Diagnostics;
using SqlServer.Configuration;
using SqlServer.Helpers;
using SqlServer.Logging;

namespace SqlServer
{
    using System.Collections.Generic;

    public static class App
    {
        public static readonly string InstanceId = StringHelper.GenerateStringShort();
        public static readonly string MachineName = Environment.MachineName;
        public static readonly int ProcessId = Create();
        public static readonly bool IsTestEnv = IsEnvVariableActive("BO_IsTestEnv");

        static int Create()
        {
            try
            {
                return Process.GetCurrentProcess().Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public static string GetEnvVariableSafe(string name)
        {
            try
            {
                return Environment.GetEnvironmentVariable(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private static bool IsEnvVariableActive(string name)
        {
            return string.Equals(GetEnvVariableSafe(name), "true", StringComparison.OrdinalIgnoreCase);
        }

        private static readonly object _locker = new object();

        private static readonly Dictionary<string, ILogger> _initLoggers = new Dictionary<string, ILogger>();

        public static IServiceProvider ServiceProvider { get; private set; }

        public static ILogger EnsureStarted(Func<IExpressApp> createApp)
        {
            lock (_locker)
            {
                var watch = Stopwatch.StartNew();
                var app = createApp();
                ILogger logger = null;
                if (!_initLoggers.ContainsKey(app.AppName))
                {
                    logger = app.Initialize();
                    logger.LogInformation("App was started. Elapsed: {Elapsed} ms", watch.ElapsedMilliseconds);
                    _initLoggers[app.AppName] = logger;
                }
                else
                {
                    logger = _initLoggers[app.AppName];
                }
                return logger;
            }
        }

        public static ILogger EnsureStarted(IExpressApp builder)
        {
            return EnsureStarted(() => builder);
        }

        public static ILogger EnsureStarted(string appName)
        {
            return EnsureStarted(() => new ExpressAppBuilder(appName));
        }
    }
}
