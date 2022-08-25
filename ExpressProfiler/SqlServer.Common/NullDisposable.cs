using System;

namespace SqlServer
{
    public class NullDisposable : IDisposable
    {
        public static readonly IDisposable Instance = new NullDisposable();

        public void Dispose()
        {

        }
    }
}
