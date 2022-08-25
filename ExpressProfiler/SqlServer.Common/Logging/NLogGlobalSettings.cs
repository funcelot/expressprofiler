using System;
using System.Runtime.CompilerServices;
using NLog.LayoutRenderers;

namespace Express.Logging
{
    internal static class NLogGlobalSettings
    {
        private static readonly ILogWriter _logWriter;

        static NLogGlobalSettings()
        {
            _logWriter = InternalLogger.Create<NLogAppLoggerBuilder>(new DiagnosticsLogWriterFactory());
            try
            {
                LayoutRenderer.Register("activityId", eventInfo => GetCurrentActivityId());
                _logWriter.Write("Inited NLogGlobalSettings");
            }
            catch (Exception ex)
            {
                _logWriter.Write("Failed init NLogGlobalSettings: {0}", ex);
            }
        }

        private static string GetCurrentActivityId()
        {
            var current = ExpressActivity.Current;

            return current != null ? current.Id : null;
        }

        public static void EnsureInited()
        {
            RuntimeHelpers.RunClassConstructor(typeof(NLogGlobalSettings).TypeHandle);
        }
    }
}
