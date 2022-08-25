using System;

namespace Express.DependencyInjection
{
    public interface IServiceScope : IDisposable
    {
        IServiceProvider ServiceProvider { get; }
    }
}
