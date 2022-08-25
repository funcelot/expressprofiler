namespace Express.Logging
{
    public interface ILogWriterFactory
    {
        ILogWriter Create(string name);
    }
}
