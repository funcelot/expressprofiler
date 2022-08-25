namespace Express.Logging
{
    public interface ILogWriter
    {
        void Write(string format, params object[] args);
    }
}
