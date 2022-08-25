using SqlServer.Logging;

namespace SqlServer.Configuration
{
    public interface IExpressApp
    {
        string AppName { get; }
        ILogger Initialize();
    }
}
