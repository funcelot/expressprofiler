using System;

namespace Express.Helpers
{
    public static class StringHelper
    {
        public static string GenerateStringShort()
        {
            return Guid.NewGuid().ToString("D").Substring(0, 8);
        }

        public static string GenerateStringLong()
        {
            return Guid.NewGuid().ToString("D");
        }
    }
}
