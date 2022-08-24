﻿using System;

namespace Wickes
{
    public static class TestErrorValidator
    {
        public static void Check(string text, string errorCode)
        {
            if (!WickesApp.IsTestEnv || text == null)
            {
                return;
            }

            var code = "ERROR:" + errorCode;
            if (text.IndexOf(code, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                throw new Exception(string.Format("Test error for {0}.", errorCode));
            }
        }
    }
}