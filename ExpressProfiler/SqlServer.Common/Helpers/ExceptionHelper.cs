using System;
using System.Reflection;
using System.Text;

namespace SqlServer.Helpers
{
    public static class ExceptionHelper
    {
        public static void Rethrow(this Exception ex)
        {
            var method = typeof(Exception).GetMethod("PrepForRemoting", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(ex, new object[0]);
            throw ex;
        }

        public static string CreateFullUserMessage(this Exception exception)
        {
            var message = new StringBuilder(exception.Message);
            var innerException = exception.InnerException;
            while (innerException != null)
            {
                message.Append(" > ");
                message.Append(innerException.Message);
                innerException = innerException.InnerException;
            }
            return message.ToString();
        }
    }
}
