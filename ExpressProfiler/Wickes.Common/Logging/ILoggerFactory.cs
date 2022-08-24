namespace Wickes.Logging
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(string name);
    }
}
