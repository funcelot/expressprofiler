namespace SqlServer.Logging
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(string name);
    }
}
