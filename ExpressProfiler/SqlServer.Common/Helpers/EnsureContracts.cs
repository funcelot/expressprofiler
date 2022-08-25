using System;

namespace SqlServer.Helpers
{
    public static class EnsureContracts
    {
        public static string EnsureNotEmpty(this string value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Expected param will be not empty.", paramName);
            }

            return value;
        }
    }
}
