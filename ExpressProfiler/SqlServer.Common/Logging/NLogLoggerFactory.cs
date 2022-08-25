using NLog;
using System.Collections.Generic;
using Express.Helpers;

namespace Express.Logging
{
    public class NLogLoggerFactory : ILoggerFactory
    {
        private readonly NLogOptions _options;
        private readonly LogFactory _logFactory;
        private readonly NLogBeginScopeParser _beginScopeParser;

        private readonly IDictionary<string, ILogger> _loggers;


        public NLogLoggerFactory(NLogOptions options, LogFactory logFactory)
        {
            _options = options;
            _logFactory = logFactory;
            _beginScopeParser = new NLogBeginScopeParser(options);
            _loggers = new Dictionary<string, ILogger>(100);
        }

        public NLogOptions Options
        {
            get
            {
                return _options;
            }
        }

        public ILogger CreateLogger(string name)
        {
            var logger = _loggers.GetOrAddWithLock(name, CreateLoggerImpl);
            return logger;
        }

        public ILogger CreateLoggerImpl(string name)
        {
            return new NLogLogger(_logFactory.GetLogger(name), _options, _beginScopeParser);
        }
    }
}
