using Wickes.Logging;

namespace Wickes.Configuration
{
    public interface IWickesApp
    {
        string AppName { get; }
        ILogger Initialize();
    }
}
