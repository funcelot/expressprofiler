namespace Express.Logging
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(string name);
    }
}
