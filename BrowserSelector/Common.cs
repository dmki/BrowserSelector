using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrowserSelector
{
    internal static class Common
    {
        public static string GetSetting(string settingName, string defaultValue = "")
        {
            var result = ConfigurationManager.AppSettings[settingName];
            return string.IsNullOrEmpty(result) ? defaultValue : result;
        }

        public static bool GetSettingBool(string settingName, bool defaultValue = false)
        {
            string val = ConfigurationManager.AppSettings[settingName];
            if (string.IsNullOrEmpty(val)) return defaultValue;
            return Boolean.TryParse(val, out var result) ? result : defaultValue;
        }

        public static Int32 GetInt32(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            value = value.Trim();
            return !int.TryParse(value, out var result) ? 0 : result;
        }
    }
}
