using System;
using System.Collections.Generic;

namespace Express.Logging
{
    public static class LogValue
    {
        private static readonly Dictionary<Type, Func<object, object>> _getLogValueRegistrations = new Dictionary<Type, Func<object, object>>();

        public static void Register<T>(Func<object, object> getLogValue)
        {
            _getLogValueRegistrations[typeof(T)] = getLogValue;
            LogMethod.Clear();
        }

        public static void RegisterDefault<T>()
        {
            Register<T>(GetLogValueDefault);
        }

        private static object GetLogValueDefault(object value)
        {
            return value;
        }

        private static bool IsLoggableSystemType(this Type type)
        {
            if (type.IsValueType)
            {
                return true;
            }

            if (type.FullName.StartsWith("System.") && !type.FullName.StartsWith("System.Data."))
            {
                return true;
            }

            return false;
        }

        public static Func<object, object> GetGetLogValue(Type type)
        {
            if (type.IsLoggableSystemType())
            {
                return GetLogValueDefault;
            }


            Func<object, object> getValue;
            if (_getLogValueRegistrations.TryGetValue(type, out getValue))
            {
                return getValue;
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                if (elementType != null && elementType.IsLoggableSystemType())
                {
                    return GetLogValueDefault;
                }
            }

            return null;

        }

    }
}
