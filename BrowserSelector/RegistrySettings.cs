using System.Reflection;
using System.Security.Permissions;
using Microsoft.Win32;

namespace BrowserSelector
{
	static class RegistrySettings
	{
		const string AppID = "DMKIBrowserSelector";
		const string AppName = "Browser Selector";
		const string AppDescription = "Browser Selector";
		static string AppPath = Assembly.GetExecutingAssembly().Location;
		static string AppIcon = AppPath + ",0";
		static string AppOpenUrlCommand = AppPath + " %1";
		static string AppReinstallCommand = AppPath + " --register";

        //[PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
		internal static void RegisterBrowser()
		{
			// Register application.
			var appReg = Registry.LocalMachine.CreateSubKey($"SOFTWARE\\{AppID}");

			// Register capabilities.
			var capabilityReg = appReg.CreateSubKey("Capabilities");
            if (capabilityReg != null)
            {
                capabilityReg.SetValue("ApplicationName", AppName);
                capabilityReg.SetValue("ApplicationIcon", AppIcon);
                capabilityReg.SetValue("ApplicationDescription", AppDescription);

                // Set up protocols we want to handle.
                var urlAssocReg = capabilityReg.CreateSubKey("URLAssociations");
                if (urlAssocReg != null)
                {
                    urlAssocReg.SetValue("http", AppID + "URL");
                    urlAssocReg.SetValue("https", AppID + "URL");
                    urlAssocReg.SetValue("ftp", AppID + "URL");
                }
            }

            // Register as application.
			Registry.LocalMachine.OpenSubKey("SOFTWARE\\RegisteredApplications", true).SetValue(AppID, $"SOFTWARE\\{AppID}\\Capabilities");

			// Set URL Handler.
			var handlerReg = Registry.LocalMachine.CreateSubKey($"SOFTWARE\\Classes\\{AppID}URL");
			handlerReg.SetValue("", AppName);
			handlerReg.SetValue("FriendlyTypeName", AppName);

			handlerReg.CreateSubKey(string.Format("shell\\open\\command", AppID)).SetValue("", AppOpenUrlCommand);
		}

        //[PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
		internal static void UnregisterBrowser()
		{
			Registry.LocalMachine.DeleteSubKeyTree($"SOFTWARE\\{AppID}", false);
			Registry.LocalMachine.OpenSubKey("SOFTWARE\\RegisteredApplications", true).DeleteValue(AppID);
			Registry.LocalMachine.DeleteSubKey($"SOFTWARE\\Classes\\{AppID}URL");
		}
	}
}
