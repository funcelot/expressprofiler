using System;
using System.Runtime.InteropServices;

namespace Express
{
    [ComVisible(true)]
    [Guid("EA6DDEC7-8FFD-4F16-AB8F-5A33337B8242")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] 
    public interface IAppEnvironment
    {
        bool IsDevEnvironment();
        bool IsTestEnvironment();
        bool IsProdEnvironment();
    }

    [ComVisible(true)]
    [Guid("DF0D3AE1-ADDC-4438-BCE9-7045433C2751")]
    public class AppEnvironment : IAppEnvironment
    {
        public bool IsDevEnvironment()
        {
            return ExpressAppEnvironment.IsDevEnvironment;
        }
        public bool IsTestEnvironment()
        {
            return ExpressAppEnvironment.IsTestEnvironment;
        }
        public bool IsProdEnvironment()
        {
            return ExpressAppEnvironment.IsProdEnvironment;
        }
    }
}
