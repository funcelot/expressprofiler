using System;

namespace Express.Logging
{
    public interface IAppLoggerBuilder
    {
        Guid Id { get; }

        ILoggerFactory CreateLoggerFactory();
    }
}
