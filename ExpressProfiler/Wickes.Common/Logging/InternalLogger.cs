namespace Wickes.Logging
{
    public static class InternalLogger
    {
        public static ILogWriter Create<T>(ILogWriterFactory logWriterFactory)
        {
            return logWriterFactory.Create(typeof(T).FullName);
        }

        public static ILogWriter Create(string name, ILogWriterFactory logWriterFactory)
        {
            return logWriterFactory.Create(name);
        }

        public class Logger
        {
            private readonly ILogWriter _logWriter;

            internal Logger(string name, ILogWriterFactory logWriterFactory)
            {
                _logWriter = logWriterFactory.Create(name);
            }


            public void Write(string format, params object[] args)
            {
                _logWriter.Write(string.Format(format, args));
            }
        }
    }
}
