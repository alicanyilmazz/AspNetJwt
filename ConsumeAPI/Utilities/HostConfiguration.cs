using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConsumeAPI.Utilities
{
    public class HostConfiguration
    {
        public static bool IsSettingExists(string key)
        {
            return false;
        }
        public static string GetSetting(string key)
        {
            return null;
        }

        public static string GetSetting(string key, string defaultValue)
        {
            return GetSetting(key) ?? defaultValue;
        }
        public static int? GetSettings(string key)
        {
            return null;
        }

        public static int GetSetting(string key, int defaultValue)
        {
            return GetSettings(key) ?? defaultValue;
        }

    }
}