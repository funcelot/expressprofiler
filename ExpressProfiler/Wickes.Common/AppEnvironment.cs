using System;
using System.Security;

namespace Wickes
{
    public static class WickesAppEnvironment
    {
        public static string GetEnvironmentVariable(string variable)
        {
            try
            {
                return Environment.GetEnvironmentVariable(variable);
            }
            catch (SecurityException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        public static bool IsEnvironment(string environmentName)
        {
            string environmentVariable = GetEnvironmentVariable("ENVIRONMENT");
            return environmentVariable != null && environmentVariable.ToLower() == "test";
        }

        public static bool IsDevEnvironment
        {
            get
            {
                try
                {
                    return IsEnvironment("dev");
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool IsTestEnvironment
        {
            get
            {
                try
                {
                    return IsEnvironment("test");
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool IsProdEnvironment
        {
            get
            {
                try
                {
                    return IsEnvironment("prod") || GetEnvironmentVariable("ENVIRONMENT") == null;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
