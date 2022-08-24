using System;

namespace Wickes.DependencyInjection
{
    public interface IServiceScope : IDisposable
    {
        IServiceProvider ServiceProvider { get; }
    }
}
