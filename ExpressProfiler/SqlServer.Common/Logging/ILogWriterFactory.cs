namespace SqlServer.Logging
{
    public interface ILogWriterFactory
    {
        ILogWriter Create(string name);
    }
}
