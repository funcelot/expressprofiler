using NLog;
using NLog.MessageTemplates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace SqlServer.Logging
{
    public static class LoggerExtensions
    {
        private static readonly IList<MessageTemplateParameter> EmptyParameterArray = new MessageTemplateParameter[] { };

        public static IDisposable BeginScope(this ILogger logger, string messageFormat, params object[] args)
        {
            return logger.BeginScope(new FormattedLogValues(messageFormat, args));
        }

        public static IDisposable BeginScopeFor(this ILogger logger, string name, object value)
        {
            return logger.BeginScope(LoggingParams.Create(name, value));
        }

        private static void CaptureMessageProperties(LogEventInfo eventInfo, IEnumerable<KeyValuePair<string, object>> messageProperties)
        {
            if (messageProperties != null)
            {
                foreach (var property in messageProperties)
                {
                    if (String.IsNullOrEmpty(property.Key))
                        continue;

                    eventInfo.Properties[property.Key] = property.Value;
                }
            }
        }

        private static LogEventInfo CreateLogEventInfo<TState>(this ILogger logger, LogLevel logLevel, TState state)
        {
            var messageProperties = (logger.Options.CaptureMessageTemplates || logger.Options.CaptureMessageProperties)
                ? state as IList<KeyValuePair<string, object>>
                : null;

            NLogMessageParameterList messageParameters;

            LogEventInfo eventInfo =
                logger.TryParseMessageTemplate(logLevel, messageProperties, out messageParameters) ??
                logger.CreateLogEventInfo(logLevel, state.ToString(), messageProperties, messageParameters);

            if (messageParameters == null && messageProperties == null && logger.Options.CaptureMessageProperties)
            {
                CaptureMessageProperties(eventInfo, state as IEnumerable<KeyValuePair<string, object>>);
            }

            return eventInfo;
        }

        private static LogEventInfo CreateLogEventInfo(this ILogger logger, LogLevel logLevel, string formattedMessage, IList<KeyValuePair<string, object>> messageProperties, NLogMessageParameterList messageParameters)
        {
            return logger.TryCaptureMessageTemplate(logLevel, formattedMessage, messageProperties, messageParameters) ??
                logger.CreateSimpleLogEventInfo(logLevel, formattedMessage, messageProperties, messageParameters);
        }

        private static readonly object[] SingleItemArray = { null };

        /// <summary>
        /// Append extra property on <paramref name="eventInfo"/>
        /// </summary>
        private static void AddExtraPropertiesToLogEvent(LogEventInfo eventInfo, List<MessageTemplateParameter> extraProperties)
        {
            if (extraProperties != null && extraProperties.Count > 0)
            {
                // Need to harvest additional parameters
                foreach (var property in extraProperties)
                    eventInfo.Properties[property.Name] = property.Value;
            }
        }

        /// <summary>
        /// Allocates object[]-array for <see cref="LogEventInfo.Parameters"/> after checking
        /// for mismatch between Microsoft Extension Logging and NLog Message Template Parser
        /// </summary>
        /// <remarks>
        /// Cannot trust the parameters received from Microsoft Extension Logging, as extra parameters can be injected
        /// </remarks>
        private static object[] CreateLogEventInfoParameters(NLogMessageParameterList messageParameters, MessageTemplateParameters messageTemplateParameters, out List<MessageTemplateParameter> extraProperties)
        {
            if (AllParameterCorrectlyPositionalMapped(messageParameters, messageTemplateParameters))
            {
                // Everything is mapped correctly, inject messageParameters directly as params-array
                extraProperties = null;
                var paramsArray = new object[messageTemplateParameters.Count];
                for (int i = 0; i < paramsArray.Length; ++i)
                    paramsArray[i] = messageParameters[i].Value;
                return paramsArray;
            }
            else
            {
                // Resolves mismatch between the input from Microsoft Extension Logging TState and NLog Message Template Parser
                if (messageTemplateParameters.IsPositional)
                {
                    return CreatePositionalLogEventInfoParameters(messageParameters, messageTemplateParameters, out extraProperties);
                }
                else
                {
                    return CreateStructuredLogEventInfoParameters(messageParameters, messageTemplateParameters, out extraProperties);
                }
            }
        }

        /// <summary>
        /// Are all parameters positional and correctly mapped?
        /// </summary>
        /// <param name="messageParameters"></param>
        /// <param name="messageTemplateParameters"></param>
        /// <returns>true if correct</returns>
        private static bool AllParameterCorrectlyPositionalMapped(NLogMessageParameterList messageParameters, MessageTemplateParameters messageTemplateParameters)
        {
            if (messageTemplateParameters.Count != messageParameters.Count || messageTemplateParameters.IsPositional)
            {
                return false;
            }

            for (int i = 0; i < messageTemplateParameters.Count; ++i)
            {
                if (messageTemplateParameters[i].Name != messageParameters[i].Name)
                {
                    return false;
                }
            }

            return true;
        }

        private static object[] CreateStructuredLogEventInfoParameters(NLogMessageParameterList messageParameters, MessageTemplateParameters messageTemplateParameters, out List<MessageTemplateParameter> extraProperties)
        {
            extraProperties = null;

            var paramsArray = new object[messageTemplateParameters.Count];
            int startPos = 0;
            for (int i = 0; i < messageParameters.Count; ++i)
            {
                bool extraProperty = true;
                for (int j = startPos; j < messageTemplateParameters.Count; ++j)
                {
                    if (messageParameters[i].Name == messageTemplateParameters[j].Name)
                    {
                        extraProperty = false;
                        paramsArray[j] = messageParameters[i].Value;
                        if (startPos == j)
                            startPos++;
                        break;
                    }
                }

                if (extraProperty)
                {
                    extraProperties = AddExtraProperty(extraProperties, messageParameters[i]);
                }
            }

            return paramsArray;
        }

        private static object[] CreatePositionalLogEventInfoParameters(NLogMessageParameterList messageParameters, MessageTemplateParameters messageTemplateParameters, out List<MessageTemplateParameter> extraProperties)
        {
            extraProperties = null;

            var maxIndex = FindMaxIndex(messageTemplateParameters);
            object[] paramsArray = null;
            for (int i = 0; i < messageParameters.Count; ++i)
            {
                // First positional name is the startPos
                if (char.IsDigit(messageParameters[i].Name[0]) && paramsArray == null)
                {
                    paramsArray = new object[maxIndex + 1];
                    for (int j = 0; j <= maxIndex; ++j)
                    {
                        if (i + j < messageParameters.Count)
                            paramsArray[j] = messageParameters[i + j].Value;
                    }
                    i += maxIndex;
                }
                else
                {
                    extraProperties = AddExtraProperty(extraProperties, messageParameters[i]);
                }
            }

            return paramsArray ?? new object[maxIndex + 1];
        }

        /// <summary>
        /// Add Property and init list if needed
        /// </summary>
        /// <param name="extraProperties"></param>
        /// <param name="item"></param>
        /// <returns>list with at least one item</returns>
        private static List<MessageTemplateParameter> AddExtraProperty(List<MessageTemplateParameter> extraProperties, MessageTemplateParameter item)
        {
            extraProperties = extraProperties ?? new List<MessageTemplateParameter>();
            extraProperties.Add(item);
            return extraProperties;
        }

        /// <summary>
        /// Find max index of the parameters
        /// </summary>
        /// <param name="messageTemplateParameters"></param>
        /// <returns>index, 0 or higher</returns>
        private static int FindMaxIndex(MessageTemplateParameters messageTemplateParameters)
        {
            int maxIndex = 0;
            for (int i = 0; i < messageTemplateParameters.Count; ++i)
            {
                if (messageTemplateParameters[i].Name.Length == 1)
                    maxIndex = Math.Max(maxIndex, messageTemplateParameters[i].Name[0] - '0');
                else
                    maxIndex = Math.Max(maxIndex, int.Parse(messageTemplateParameters[i].Name));
            }

            return maxIndex;
        }

        /// <summary>
        /// Checks if the already parsed input message-parameters must be sent through
        /// the NLog MessageTemplate Parser for proper handling of message-template syntax.
        /// </summary>
        /// <remarks>
        /// Using the NLog MessageTemplate Parser will hurt performance: 1 x Microsoft Parser - 2 x NLog Parser - 1 x NLog Formatter
        /// </remarks>
        private static LogEventInfo TryParseMessageTemplate(this ILogger logger, LogLevel logLevel, IList<KeyValuePair<string, object>> messageProperties, out NLogMessageParameterList messageParameters)
        {
            messageParameters = logger.TryParseMessageParameterList(messageProperties);

            if (messageParameters != null && messageParameters.HasMessageTemplateSyntax(logger.Options.ParseMessageTemplates))
            {
                var originalMessage = messageParameters.GetOriginalMessage(messageProperties);
                var eventInfo = new LogEventInfo(logLevel, logger.Name, null, originalMessage, SingleItemArray);
                var messageTemplateParameters = eventInfo.MessageTemplateParameters;   // Forces parsing of OriginalMessage
                if (messageTemplateParameters.Count > 0)
                {
                    List<MessageTemplateParameter> extraProperties;
                    // We have parsed the message and found parameters, now we need to do the parameter mapping
                    eventInfo.Parameters = CreateLogEventInfoParameters(messageParameters, messageTemplateParameters, out extraProperties);
                    AddExtraPropertiesToLogEvent(eventInfo, extraProperties);
                    return eventInfo;
                }

                return null;    // Parsing not possible
            }

            return null;    // Parsing not needed
        }

        /// <summary>
        /// Convert IReadOnlyList to <see cref="NLogMessageParameterList"/>
        /// </summary>
        /// <param name="messageProperties"></param>
        /// <returns></returns>
        private static NLogMessageParameterList TryParseMessageParameterList(this ILogger logger, IList<KeyValuePair<string, object>> messageProperties)
        {
            return (messageProperties != null && logger.Options.CaptureMessageTemplates)
                ? NLogMessageParameterList.TryParse(messageProperties)
                : null;
        }

        private static LogEventInfo CreateSimpleLogEventInfo(this ILogger logger, LogLevel logLevel, string message, IList<KeyValuePair<string, object>> messageProperties, NLogMessageParameterList messageParameters)
        {
            // Parsing failed or no messageParameters
            var eventInfo = LogEventInfo.Create(logLevel, logger.Name, message);
            if (messageParameters != null)
            {
                for (int i = 0; i < messageParameters.Count; ++i)
                {
                    var property = messageParameters[i];
                    eventInfo.Properties[property.Name] = property.Value;
                }
            }
            else if (messageProperties != null && logger.Options.CaptureMessageProperties)
            {
                CaptureMessagePropertiesList(eventInfo, messageProperties);
            }
            return eventInfo;
        }

        private static void CaptureMessagePropertiesList(LogEventInfo eventInfo, IList<KeyValuePair<string, object>> messageProperties)
        {
            for (int i = 0; i < messageProperties.Count; ++i)
            {
                var property = messageProperties[i];
                if (String.IsNullOrEmpty(property.Key))
                    continue;

                if (i == messageProperties.Count - 1 && NLogLogger.OriginalFormatPropertyName.Equals(property.Key))
                    continue;

                eventInfo.Properties[property.Key] = property.Value;
            }
        }

        private static LogEventInfo TryCaptureMessageTemplate(this ILogger logger, LogLevel logLevel, string message, IList<KeyValuePair<string, object>> messageProperties, NLogMessageParameterList messageParameters)
        {
            if (messageParameters != null && !messageParameters.HasComplexParameters)
            {
                // Parsing not needed, we take the fast route 
                var originalMessage = messageParameters.GetOriginalMessage(messageProperties);
                var eventInfo = new LogEventInfo(logLevel, logger.Name, originalMessage ?? message, messageParameters.IsPositional ? EmptyParameterArray : messageParameters);
                if (originalMessage != null)
                {
                    SetLogEventMessageFormatter(eventInfo, messageParameters, message);
                }
                return eventInfo;
            }
            return null;
        }

        private static void SetLogEventMessageFormatter(LogEventInfo logEvent, NLogMessageParameterList messageTemplateParameters, string formattedMessage)
        {
            var parameters = new object[messageTemplateParameters.Count + 1];
            for (int i = 0; i < parameters.Length - 1; ++i)
                parameters[i] = messageTemplateParameters[i].Value;
            parameters[parameters.Length - 1] = formattedMessage;
            logEvent.Parameters = parameters;
            logEvent.MessageFormatter = (l) => (string)l.Parameters[l.Parameters.Length - 1];
        }

        public static void Log<TState>(this ILogger logger, LogLevel logLevel, TState state, Exception exception)
        {
            try
            {
                var watch = Stopwatch.StartNew();

                if (!logger.IsEnabled(logLevel))
                {
                    return;
                }

                LogEventInfo eventInfo = logger.CreateLogEventInfo(logLevel, state);

                if (exception != null)
                {
                    eventInfo.Exception = exception;
                }

                logger.Log(typeof(ILogger), eventInfo);

                LogPerformanceWatcher.ReportLogged(watch.ElapsedTicks);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                //make logging safe, don't rethrow it
                Debug.WriteLine(ex.Message);
            }
        }

        public static void Log(this ILogger logger, Type type, LogEventInfo eventInfo)
        {
            logger.Log(type, eventInfo);
        }

        public static void Log(this ILogger logger, LogLevel level, Exception exception, string message, params object[] args)
        {
            logger.Log(level, new FormattedLogValues(message, args), exception);
        }

        public static void Log(this ILogger logger, LogLevel level, string message, params object[] args)
        {
            logger.Log(level, null, message, args);
        }

        public static void LogTrace(this ILogger logger, Exception exception, string message, params object[] args)
        {
            logger.Log(LogLevel.Trace, exception, message, args);
        }

        public static void LogTrace(this ILogger logger, string message, params object[] args)
        {
            logger.Log(LogLevel.Trace, message, args);
        }


        public static void LogDebug(this ILogger logger, Exception exception, string message, params object[] args)
        {
            logger.Log(LogLevel.Debug, exception, message, args);
        }

        public static void LogDebug(this ILogger logger, string message, params object[] args)
        {
            logger.Log(LogLevel.Debug, message, args);
        }


        public static void LogInformation(this ILogger logger, Exception exception, string message, params object[] args)
        {
            logger.Log(LogLevel.Info, exception, message, args);
        }

        public static void LogInformation(this ILogger logger, string message, params object[] args)
        {
            logger.Log(LogLevel.Info, message, args);
        }

        public static void LogWarning(this ILogger logger, Exception exception, string message, params object[] args)
        {
            logger.Log(LogLevel.Warn, exception, message, args);
        }

        public static void LogWarning(this ILogger logger, string message, params object[] args)
        {
            logger.Log(LogLevel.Warn, message, args);
        }


        public static void LogError(this ILogger logger, Exception exception, string message, params object[] args)
        {
            logger.Log(LogLevel.Error, exception, message, args);
        }

        public static void LogError(this ILogger logger, string message, params object[] args)
        {
            logger.Log(LogLevel.Error, message, args);
        }

        public static void LogCritical(this ILogger logger, Exception exception, string message, params object[] args)
        {
            logger.Log(LogLevel.Fatal, exception, message, args);
        }

        public static void LogCritical(this ILogger logger, string message, params object[] args)
        {
            logger.Log(LogLevel.Fatal, message, args);
        }

        public static void LogAppStarted(this ILogger logger, string appName)
        {
            var process = Process.GetCurrentProcess();

            logger.LogInformation("Starting app '{AppName}' for process '{ProcessName}', processId: '{ProcessId}' on machine '{MachineName}'.",
                appName, process.ProcessName, process.Id, ExpressApp.MachineName);

            logger.LogInformation("Process CommandLine: '{CommandLine}'.", Environment.CommandLine);
            logger.LogInformation("Current user: '{UserName}', Domain: '{UserDomainName}'.", Environment.UserName, Environment.UserDomainName);
            logger.LogInformation("CurrentDirectory: '{CurrentDirectory}'.", Environment.CurrentDirectory);
            logger.LogInformation("ExpressApp InstanceId: '{InstanceId}', IsTestEnv: '{IsTestEnv}'.", ExpressApp.InstanceId, ExpressApp.IsTestEnv);

            var domain = AppDomain.CurrentDomain;
            logger.LogInformation("Domain BaseDirectory: '{BaseDirectory}', RelativeSearchPath: '{RelativeSearchPath}'", domain.BaseDirectory, domain.RelativeSearchPath);

            var commonAssembly = typeof(AppLogger).Assembly;
            logger.LogInformation("Common assembly '{Assembly}' has FileVersion: '{FileVersion}', CodeBase: '{CodeBase}', Location: '{Location}'.",
                commonAssembly.GetName().Name, GetFileVersion(commonAssembly.Location), commonAssembly.CodeBase, commonAssembly.Location);
        }

        public static void LogAppStartedSafe(this ILogger logger, string appName)
        {
            try
            {
                logger.LogAppStarted(appName);
            }
            catch (Exception ex)
            {
                NLog.Common.InternalLogger.Debug(ex, "Failed to run LogAppStarted.");
            }
        }

        private static string GetFileVersion(string path)
        {
            try
            {
                var info = FileVersionInfo.GetVersionInfo(path);
                return info.FileVersion;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
