using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace SqlServer.Helpers
{
    public static class LibraryHelper
    {
        public static string GetLibaryName()
        {
            return Assembly.GetEntryAssembly() != null ?
                Assembly.GetEntryAssembly().GetName().Name.Replace(".", "") :
                Process.GetCurrentProcess().ProcessName.Replace(".", "");
        }

        public static int GetManagedThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }
    }
}
