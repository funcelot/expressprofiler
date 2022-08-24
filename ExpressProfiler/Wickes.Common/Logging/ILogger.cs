﻿using NLog;
using System;

namespace Wickes.Logging
{
    public interface ILogger
    {
        string Name { get; }
        bool IsEnabled(LogLevel logLevel);
        NLogOptions Options { get; }
        IDisposable BeginScope<TState>(TState state);
        void Log(Type type, LogEventInfo eventInfo);
    }
}