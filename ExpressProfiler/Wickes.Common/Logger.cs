using System;
using Express.Logging;
using Express.Helpers;

namespace Express.Common
{
    public static class Logger
    {
        private static ILogger _logger;

        public static ILogger Instance { get { return _logger; } }


        static Logger()
        {
            _logger = LibraryAppLogger.Initialize(LibraryHelper.GetLibaryName());
        }

        public static void LogTrace(Exception exception, string message, params object[] args)
        {
            _logger.LogTrace(exception, message, args);
        }

        public static void LogTrace(string message, params object[] args)
        {
            _logger.LogTrace(message, args);
        }

        public static void LogDebug(Exception exception, string message, params object[] args)
        {
            _logger.LogDebug(exception, message, args);
        }

        public static void LogDebug(string message, params object[] args)
        {
            _logger.LogDebug(message, args);
        }

        public static void LogInformation(Exception exception, string message, params object[] args)
        {
            _logger.LogInformation(exception, message, args);
        }

        public static void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }

        public static void LogWarning(Exception exception, string message, params object[] args)
        {
            _logger.LogWarning(exception, message, args);
        }

        public static void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }

        public static void LogError(Exception exception, string message, params object[] args)
        {
            _logger.LogError(exception, message, args);
        }

        public static void LogError(string message, params object[] args)
        {
            _logger.LogError(message, args);
        }

        public static void LogCritical(Exception exception, string message, params object[] args)
        {
            _logger.LogCritical(exception, message, args);
        }

        public static void LogCritical(string message, params object[] args)
        {
            _logger.LogCritical(message, args);
        }
    }
}
