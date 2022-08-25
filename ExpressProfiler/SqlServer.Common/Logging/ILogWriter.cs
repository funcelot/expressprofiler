namespace SqlServer.Logging
{
    public interface ILogWriter
    {
        void Write(string format, params object[] args);
    }
}
