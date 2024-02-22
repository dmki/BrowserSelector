using System;
using Microsoft.Win32;

namespace BrowserSelector
{
    public static class RegHelper
    {
        private const string HKCU = @"HKEY_CURRENT_USER\";
        private const string HKLM = @"HKEY_LOCAL_MACHINE\";
        private const string SettingsPath = @"SOFTWARE\DMKI\Browser\";

        public static string GetSettingString(string settingName, string defaultValue = "", RegistryRootType root = RegistryRootType.HKEY_CURRENT_USER)
        {
            string registryRoot = (root == RegistryRootType.HKEY_CURRENT_USER) ? HKCU : HKLM;
            var result = Convert.ToString(Registry.GetValue(registryRoot + SettingsPath, settingName, string.Empty));
            return string.IsNullOrEmpty(result) ? defaultValue : result;
        }

        public static Int32 GetSettingInt(string settingName, int defaultValue = 0, RegistryRootType root = RegistryRootType.HKEY_CURRENT_USER)
        {
            string registryRoot = (root == RegistryRootType.HKEY_CURRENT_USER) ? HKCU : HKLM;
            return Convert.ToInt32(Registry.GetValue(registryRoot + SettingsPath, settingName, defaultValue));
        }

        public static Int32 GetSettingInt2(string settingName, int defaultValue = 0, int minValue = 0, int maxValue = 100, RegistryRootType root = RegistryRootType.HKEY_CURRENT_USER)
        {
            var myVal = GetSettingInt(settingName, defaultValue, root);
            if (myVal < minValue) myVal = minValue;
            if (myVal > maxValue) myVal = maxValue;
            return myVal;
        }

        public static bool GetSettingBool(string settingName, RegistryRootType root = RegistryRootType.HKEY_CURRENT_USER)
        {
            string registryRoot = (root == RegistryRootType.HKEY_CURRENT_USER) ? HKCU : HKLM;
            return Convert.ToBoolean(Registry.GetValue(registryRoot + SettingsPath, settingName, 0));
        }

        public static long GetSettingInt64(string settingName, RegistryRootType root = RegistryRootType.HKEY_CURRENT_USER)
        {
            string registryRoot = (root == RegistryRootType.HKEY_CURRENT_USER) ? HKCU : HKLM;
            return Convert.ToInt64(Registry.GetValue(registryRoot + SettingsPath, settingName, 0));
        }

        public static void SaveSetting(string settingName, object value,
                                RegistryRootType root = RegistryRootType.HKEY_CURRENT_USER)
        {
            string registryRoot = (root == RegistryRootType.HKEY_CURRENT_USER) ? HKCU : HKLM;
            Registry.SetValue(registryRoot + SettingsPath, settingName, value);
        }

        //public static void DeleteSetting(string wsKey)
        //{
        //    string keyName = SettingsPath + wsKey;
        //    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName, true))
        //    {
        //        if (key == null)
        //        {
        //            // Key doesn't exist. Do whatever you want to handle
        //            // this case
        //        }
        //        else
        //        {
        //            key.DeleteValue("MyApp");
        //        }
        //    }
        //}
        public static void DeleteSetting(string valueName, RegistryRootType root = RegistryRootType.HKEY_CURRENT_USER)
        {
            string registryRoot = (root == RegistryRootType.HKEY_CURRENT_USER) ? HKCU : HKLM;
            using var key = Registry.CurrentUser.OpenSubKey(registryRoot + SettingsPath, true);
            key?.DeleteValue(valueName);
        }

        public static void DeleteAllSettings()
        {
            var root = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\DMKI");
            try
            {
                root?.DeleteSubKey("SoundWorks", false);
            }
            catch (Exception)
            {

            }
        }

        public static byte GetSettingByte(string settingName, RegistryRootType root = RegistryRootType.HKEY_CURRENT_USER)
        {
            string registryRoot = (root == RegistryRootType.HKEY_CURRENT_USER) ? HKCU : HKLM;
            return Convert.ToByte(Registry.GetValue(registryRoot + SettingsPath, settingName, 0));
        }
    }

    public enum RegistryRootType
    {
        HKEY_LOCAL_MACHINE,
        HKEY_CURRENT_USER
    }
}