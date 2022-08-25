using Express.Logging;

namespace Express.Configuration
{
    public interface IExpressApp
    {
        string AppName { get; }
        ILogger Initialize();
    }
}
