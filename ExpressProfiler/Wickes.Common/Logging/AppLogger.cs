using System;
using System.Collections.Generic;
using Express.Configuration;
using Express.Helpers;

namespace Express.Logging
{
    public sealed class AppLogger
    {
        private static readonly ILogWriter _logWriter;

        static AppLogger()
        {
            _logWriter = InternalLogger.Create<AppLogger>(new DiagnosticsLogWriterFactory());
            _logWriter.Write("cctor");
        }

        private AppLogger() { }

        private static bool _inited = false;
        private static readonly object _locker = new object();
        private static ILogger _logger;

        public static ILogger EnsureInited<T>(string loggerName)
            where T : IAppLoggerBuilder, new()
        {
            lock (_locker)
            {
                if (_inited)
                {
                    return _logger;
                }

                _inited = true;
                _logger = InitLoggingSafe<T>(loggerName);
                return _logger;
            }
        }
        public static ILogger EnsureInited(IAppLoggerBuilder builder)
        {
            lock (_locker)
            {
                if (_initLogger.ContainsKey(builder.Id))
                {
                    return _initLogger[builder.Id];
                }

                _logger = InitLoggingSafe(builder);
                _initLogger[builder.Id] = _logger;
                return _logger;
            }
        }

        private static readonly Dictionary<Guid, ILogger> _initLogger = new Dictionary<Guid, ILogger>();

        private static ILogger InitLoggingSafe<T>(string loggerName)
            where T : IAppLoggerBuilder, new()
        {
            try
            {
                var builder = new T();
                var factory = builder.CreateLoggerFactory();
                var logger = factory.CreateLogger(loggerName);
                return logger;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private static ILogger InitLoggingSafe(IAppLoggerBuilder builder)
        {
            try
            {
                var factory = builder.CreateLoggerFactory();
                var logger = Init(factory);
                LogPerformanceWatcher.Init(factory);
                return logger;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ExceptionHelper.CreateFullUserMessage(ex));
                throw;
            }
        }

        internal static readonly Dictionary<int, ILoggerFactory> Factories = new Dictionary<int, ILoggerFactory>();

        public static ILogger Init(ILoggerFactory loggerFactory)
        {
#if DEBUG
            _logWriter.Write("Initing AppLogger by {0} from:\r\n{1}", loggerFactory.GetType().FullName, Environment.StackTrace);
#else
            _logWriter.Write("Initing AppLogger by {0}", loggerFactory.GetType().FullName);
#endif
            SetLoggerFactory(loggerFactory);
            return loggerFactory.CreateLogger("App");
        }

        public static ILogger CreateLogger<T>(ILoggerFactory loggerFactory)
        {
            return CreateLogger(loggerFactory, typeof(T));
        }

        public static ILogger CreateLogger(ILoggerFactory loggerFactory, string applicationName)
        {
            return loggerFactory.CreateLogger(applicationName);
        }

        public static ILogger CreateLogger(ILoggerFactory loggerFactory, Type type)
        {
            return loggerFactory.CreateLogger(type.FullName);
        }

        public static ILogger CreateLogger<T>()
        {
            return CreateLogger(typeof(T));
        }

        public static ILogger CreateLogger<T>(string appLoggerName) where T : IExpressApp
        {
            try
            {
                ILoggerFactory factory = GetLoggerFactory();
                if (factory != null)
                {
                    return factory.CreateLogger(appLoggerName);
                }
                else
                {
                    var app = (T)Activator.CreateInstance(typeof(T), appLoggerName);
                    return app.Initialize();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Environment.ExitCode = 1;
            }
            return null;
        }

        public static ILogger CreateLogger(Type type)
        {
            return CreateLogger<DefaultAppBuilder>(type.FullName);
        }

        public static ILogger CreateLogger(string appLoggerName)
        {
            return CreateLogger<DefaultAppBuilder>(appLoggerName);
        }

        public static ILogger Initialize<T>(string appName) where T : IAppLoggerBuilder, new()
        {
            var logger = EnsureInited<T>(appName);
            return logger;
        }

        public static ILoggerFactory GetLoggerFactory()
        {
            var id = LibraryHelper.GetManagedThreadId();
            ILoggerFactory factory = Factories.ContainsKey(id) ? Factories[id] : null;
            return factory;
        }

        public static void SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            if (loggerFactory != null)
            {
                var id = LibraryHelper.GetManagedThreadId();
                Factories[id] = loggerFactory;
            }
        }

        public static IList<KeyValuePair<string, object>> CreateLoggingParams()
        {
            return new LoggingParams();
        }
    }
}
