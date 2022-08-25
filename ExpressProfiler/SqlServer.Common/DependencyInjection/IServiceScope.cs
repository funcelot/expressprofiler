using System;

namespace SqlServer.DependencyInjection
{
    public interface IServiceScope : IDisposable
    {
        IServiceProvider ServiceProvider { get; }
    }
}
