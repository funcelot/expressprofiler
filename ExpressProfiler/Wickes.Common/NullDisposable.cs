using System;

namespace Wickes
{
    public class NullDisposable : IDisposable
    {
        public static readonly IDisposable Instance = new NullDisposable();

        public void Dispose()
        {

        }
    }
}
