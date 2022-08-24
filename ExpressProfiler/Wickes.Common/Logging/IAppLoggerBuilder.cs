using System;

namespace Wickes.Logging
{
    public interface IAppLoggerBuilder
    {
        Guid Id { get; }

        ILoggerFactory CreateLoggerFactory();
    }
}
