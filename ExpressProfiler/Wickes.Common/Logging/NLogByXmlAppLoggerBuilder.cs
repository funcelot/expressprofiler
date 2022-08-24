using NLog;
using NLog.Config;
using System;
using System.IO;
using System.Xml;
using Wickes.Resources;

namespace Wickes.Logging
{
    public class NLogByXmlAppLoggerBuilder : NLogAppLoggerBuilder
    {
        private static readonly ILogWriter _logWriter;

        static NLogByXmlAppLoggerBuilder()
        {
            _logWriter = InternalLogger.Create<NLogByXmlAppLoggerBuilder>(new DiagnosticsLogWriterFactory());
        }

        private readonly INLogVariablesResolver _resolver;

        public NLogByXmlAppLoggerBuilder()
        {
            _resolver = null;
        }

        public NLogByXmlAppLoggerBuilder(INLogVariablesResolver resolver)
        {
            _resolver = resolver;
        }

        public NLogByXmlAppLoggerBuilder(string appName)
            : this(new NLogVariablesResolver(appName))
        {
        }


        protected override LogFactory CreateLogFactory(NLogOptions options)
        {
            var factory = base.CreateLogFactory(options);
            var xmlConfig = LoadXmlConfig(options);
            _logWriter.Write("Loaded NLog xml config:\r\n{0}", xmlConfig);
            using (var reader = XmlReader.Create(new StringReader(xmlConfig)))
            {
                factory.Configuration = new XmlLoggingConfiguration(reader, null, factory);
            }
            OverrideVariables(factory);
            return factory;
        }

        protected override NLogOptions CreateNLogOptions()
        {
            var options = new NLogOptions();
            //options.ThrowExceptions = true;
            options.ThrowConfigExceptions = true;
            options.NLogFileName = "nlog.config";
            return options;
        }

        protected virtual string LoadXmlConfig(NLogOptions options)
        {
            string config;
            if (_resolver != null)
            {
                config = _resolver.Resolve("config");
                if (!string.IsNullOrEmpty(config))
                {
                    _logWriter.Write("Resolved nlog.config from variables.");
                    return config;
                }
            }

            config = WickesResourceManager.ReadCommon("nlog.config");
            return config;
        }

        protected virtual void OverrideVariables(LogFactory logFactory)
        {
            if (_resolver == null)
            {
                return;
            }

            _logWriter.Write("Begin OverrideVariables");

            foreach (var key in logFactory.Configuration.Variables.Keys)
            {
                _logWriter.Write("Detected variable '{0}'", key);
                try
                {
                    var value = _resolver.Resolve(key);
                    if (!string.IsNullOrEmpty(value))
                    {
                        _logWriter.Write("Overriding variable '{0}' value to '{1}'", key, value);
                        logFactory.Configuration.Variables[key] = value;
                    }
                }
                catch (Exception ex)
                {
                    _logWriter.Write("Exception: {0}", ex.Message);
                }
            }
        }
    }
}
