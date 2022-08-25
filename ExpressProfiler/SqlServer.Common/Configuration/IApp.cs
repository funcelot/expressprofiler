using SqlServer.Logging;

namespace SqlServer.Configuration
{
    public interface IApp
    {
        string AppName { get; }
        ILogger Initialize();
    }
}
