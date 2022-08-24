namespace Wickes.Logging
{
    public interface ILogWriterFactory
    {
        ILogWriter Create(string name);
    }
}
