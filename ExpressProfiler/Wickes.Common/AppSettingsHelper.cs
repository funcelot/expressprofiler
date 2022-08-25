using System;
using System.Configuration;
using Wickes.Helpers;

namespace Wickes
{
    public static class AppSettingsHelper
    {
        public static string GetSettingsOrDefault(string name, string defaultValue = null)
        {
            try
            {
                string value = ConfigurationManager.AppSettings[name];

                if (value != null)
                {
                    value = value.Trim();
                }

                if (string.IsNullOrEmpty(value))
                {
                    return defaultValue;
                }
                return value;
            }
            catch (ConfigurationErrorsException ex) 
            {
                Console.WriteLine(ExceptionHelper.CreateFullUserMessage(ex));
                throw;
            }
            catch
            {
                return null;
            }
        }

        public static string GetSettings(string name)
        {
            string value = GetSettingsOrDefault(name);

            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException(string.Format("Not found appSetting for parameter '{0}'.", name));
            }

            return value.Trim();
        }

        public static int? GetSettingsAsIntOrDefault(string name)
        {
            string value = GetSettingsOrDefault(name);

            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Bad int settings for parameter '{0}'.", name), ex);
            }
        }

        public static bool? GetSettingsAsBooleanOrDefault(string name)
        {
            string value = GetSettingsOrDefault(name);

            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            try
            {
                return Convert.ToBoolean(value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Bad int settings for parameter '{0}'.", name), ex);
            }
        }

        public static int GetSettingsAsInt(string name)
        {
            string value = GetSettings(name);

            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Bad int settings for parameter '{0}'.", name), ex);
            }
        }

        public static bool GetSettingsAsBool(string name)
        {
            string value = GetSettings(name);

            try
            {
                return Convert.ToBoolean(value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Bad Bool settings for parameter '{0}'.", name), ex);
            }
        }

        public static TimeSpan GetSettingsAsTimeSpan(string name)
        {
            string value = GetSettings(name);

            try
            {
                return TimeSpan.Parse(value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Bad TimeSpan settings for parameter '{0}'.", name), ex);
            }
        }
    }
}
