namespace Express.Logging
{
    /// <summary>
    /// Options for logging to NLog with 
    /// </summary>
    public class NLogOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether exceptions should be thrown. See also <see cref="P:NLog.LogFactory.ThrowConfigExceptions" />.
        /// </summary>
        /// <value>A value of <c>true</c> if exception should be thrown; otherwise, <c>false</c>.</value>
        /// <remarks>By default exceptions are not thrown under any circumstances.</remarks>
        public bool? ThrowExceptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="T:NLog.NLogConfigurationException" /> should be thrown.
        ///
        /// If <c>null</c> then <see cref="P:NLog.LogFactory.ThrowExceptions" /> is used.
        /// </summary>
        /// <value>A value of <c>true</c> if exception should be thrown; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// This option is for backwards-compatibility.
        /// By default exceptions are not thrown under any circumstances.
        /// </remarks>
        public bool? ThrowConfigExceptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Variables should be kept on configuration reload.
        /// Default value - false.
        /// </summary>
        public bool? KeepVariablesOnReload { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically call <see cref="M:NLog.LogFactory.Shutdown" />
        /// on AppDomain.Unload or AppDomain.ProcessExit
        /// </summary>
        public bool? AutoShutdown { get; set; }

        /// <summary>
        /// Gets or sets the current logging configuration. After setting this property all
        /// existing loggers will be re-configured, so there is no need to call <see cref="M:NLog.LogFactory.ReconfigExistingLoggers" />
        /// manually.
        /// </summary>
        public NLog.Config.LoggingConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the global log level threshold. Log events below this threshold are not logged.
        /// </summary>
        public NLog.LogLevel GlobalThreshold { get; set; }

        /// <summary>
        /// Separator between for EventId.Id and EventId.Name. Default to _
        /// </summary>
        private string _eventIdSeparator = "_";
        public string EventIdSeparator { get { return _eventIdSeparator; } set { _eventIdSeparator = value; } }

        /// <summary>
        /// Skip allocation of <see cref="LogEventInfo.Properties" />-dictionary
        /// </summary>
        /// <remarks>
        /// using
        ///     <c>default(EventId)</c></remarks>
        private bool _ignoreEmptyEventId = true;
        public bool IgnoreEmptyEventId { get { return _ignoreEmptyEventId; } set { _ignoreEmptyEventId = value; } }

        /// <summary>
        /// Enable structured logging by capturing message template parameters and inject into the <see cref="LogEventInfo.Properties" />-dictionary
        /// </summary>
        public bool _captureMessageTemplates = true;
        public bool CaptureMessageTemplates { get { return _captureMessageTemplates; } set { _captureMessageTemplates = value; } }

        /// <summary>
        /// Enable capture of properties from the ILogger-State-object, both in <see cref="Microsoft.Extensions.Logging.ILogger.Log{TState}"/> and <see cref="Microsoft.Extensions.Logging.ILogger.BeginScope{TState}"/>
        /// </summary>
        public bool _captureMessageProperties = true;
        public bool CaptureMessageProperties { get { return _captureMessageProperties; } set { _captureMessageProperties = value; } }

        /// <summary>
        /// Use the NLog engine for parsing the message template (again) and format using the NLog formatter
        /// </summary>
        public bool ParseMessageTemplates { get; set; }

        /// <summary>
        /// Enable capture of scope information and inject into <see cref="NestedDiagnosticsLogicalContext" /> and <see cref="MappedDiagnosticsLogicalContext" />
        /// </summary>
        private bool _includeScopes = true;
        public bool IncludeScopes { get { return _includeScopes; } set { _includeScopes = value; } }

        /// <summary>
        /// Shutdown NLog on dispose of the <see cref="NLogLoggerProvider"/>
        /// </summary>
        public bool ShutdownOnDispose { get; set; }

        /// <summary>
        /// NLog file name
        /// </summary>
        public string NLogFileName { get; set; }

        /// <summary>Initializes a new instance NLogOptions with default values.</summary>
        public NLogOptions()
        {
        }

        /// <summary>
        /// Default options
        /// </summary>
        internal static readonly NLogOptions Default = new NLogOptions();
    }
}
