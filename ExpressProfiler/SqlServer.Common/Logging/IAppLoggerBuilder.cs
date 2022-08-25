using System;

namespace SqlServer.Logging
{
    public interface IAppLoggerBuilder
    {
        Guid Id { get; }

        ILoggerFactory CreateLoggerFactory();
    }
}
