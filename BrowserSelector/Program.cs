using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using DanTup.BrowserSelector;

namespace BrowserSelector
{
	class Program
    {
        private static bool automaticMode = false;
        private static bool editMode = false;
		static void Main(string[] args)
		{
            bool waitForClose = false;

            if (args == null || args.Length == 0)
			{
				ShowHelpInfo();
				return;
			}

			foreach (var s in args)
            {
                var arg = s.Trim();

                var isOption = arg.StartsWith("-") || arg.StartsWith("/");
                while (arg.StartsWith("-") || arg.StartsWith("/"))
                    arg = arg.Substring(1);

                if (isOption)
                {
                    if (string.Equals(arg, "register", StringComparison.OrdinalIgnoreCase))
                    {
                        EnsureAdmin("--" + arg);
                        RegistrySettings.RegisterBrowser();
                        return;
                    }

                    if (string.Equals(arg, "edit", StringComparison.InvariantCultureIgnoreCase))
                    {
                        editMode = true;
                        continue;
                    }

                    if (string.Equals(arg, "unregister", StringComparison.OrdinalIgnoreCase))
                    {
                        EnsureAdmin("--" + arg);
                        RegistrySettings.UnregisterBrowser();
                        return;
                    }

                    if (string.Equals(arg, "create", StringComparison.OrdinalIgnoreCase))
                    {
                        CreateSampleSettings();
                        return;
                    }

                    if (string.Equals(arg, "wait", StringComparison.InvariantCultureIgnoreCase))
                    {
                        waitForClose = true;
                    }
                    else if (string.Equals(arg, "no-wait", StringComparison.InvariantCultureIgnoreCase))
                    {
                        waitForClose = false;
                    }
                }
                else
                {
                    if (arg.StartsWith("file://", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
                    {
                        LaunchBrowser(arg, waitForClose);
                    }
                    else if (arg.EndsWith(".url", StringComparison.InvariantCultureIgnoreCase) || arg.EndsWith(".website", StringComparison.InvariantCultureIgnoreCase))
                    {
                        LaunchUrlFile(arg, waitForClose);
                    }
                    else if (arg.EndsWith(".webloc", StringComparison.InvariantCultureIgnoreCase))
                    {
                        LaunchWeblocFile(arg, waitForClose);
                    }
					else if (arg.EndsWith(".wlt", StringComparison.InvariantCultureIgnoreCase))
                    {//Location list!
                        LoadLocationList(arg);
                    }
                    else
                    {
                        ShowHelpInfo();
                        return;
                    }
                }
            }
		}

        private static void LoadLocationList(string fileName)
        {
            if (!File.Exists(fileName)) return;
            //Edit?
            if (editMode)
            {
                Command cmd = new Command {Line = @"notepad.exe", Arguments = fileName, WaitForExit = false};
                cmd.Execute();
                return;
            }
            automaticMode = true;
			//Open file and read it line by line. # means comment, @ means setting
            var lines = File.ReadAllLines(fileName);
            int delay = 500;
            bool waitForClose = false;
            bool isFirst = true;
            var linkPrefix = new List<string> {"http", "https", "file"};
            foreach (var line in lines)
            {
                var s = line.Trim();
				if (string.IsNullOrEmpty(s) || s.StartsWith("#")) continue;
                if (s.StartsWith("@"))
                {// delay,browser, wait. e.g. @delay 1000
                    s = s.ToLowerInvariant().Replace('=', ' ');
                    var parts = s.Split(' ');
                    if (s.StartsWith("@delay"))
                    {
                        if (parts.Length == 1) continue;
                        delay = Common.GetInt32(parts.Last());
                        if (delay == 0) delay = 100;
                        if (delay > 60000) delay = 60000;
                    }

                    if (s.StartsWith("@wait"))
                    {
                        waitForClose = parts.Length == 1;
                    }
                }
                //Check if line begins with allowed link prefix
                if (!linkPrefix.Any(x => s.StartsWith(x))) continue;
                LaunchBrowser(line, waitForClose);
                //Delay?
                if (isFirst)
                {//For the very first invocation - wait 1 second
                    Thread.Sleep(1000);
                    isFirst = false;
                }
                if (delay > 0) Thread.Sleep(delay);
            }
        }

        static void ShowHelpInfo()
        {
            (new frmNag()).ShowDialog();
//			MessageBox.Show(@"Usage:

//	BrowserSelector.exe --register
//		Register as web browser

//	BrowserSelector.exe --unregister
//		Unregister as web browser

//	BrowserSelector.exe --create
//		Creates a default/sample settings file

//Once you have registered the app as a browser, you should use visit ""Set Default Browser"" in Windows to set this app as the default browser.

//	BrowserSelector.exe ""http://example.org/""
//		Launch example.org

//	BrowserSelector.exe [--wait] ""http://example.org/""
//		Launch example.org, optionally waiting for the browser to close..

//	BrowserSelector.exe ""http://example.org/"" ""http://example.com/"" [...]
//		Launches multiple urls

//	BrowserSelector.exe ""my bookmark file.url""
//		Launches the URL specified in the .url file

//	BrowserSelector.exe ""my bookmark file.webloc""
//		Launches the URL specified in the .webloc (osx) file

//If you use the --wait flag with multiple urls/files each will open one after the other, in order. Each waits for the previous to close before opening. Using the --wait flag is tricky, though, since many (most) browsers open new urls as a new tab in an existing instance.

//To open multiple urls at the same time and wait for them, try the following:

//	BrowserSelector.exe ""url-or-file"" ""url-or-file"" --wait ""url-or-file""", "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		static void EnsureAdmin(string arg)
		{
			WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
			if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = Assembly.GetExecutingAssembly().Location,
					Verb = "runas",
					Arguments = arg
				});
				Environment.Exit(0);
			}
		}


		static void LaunchUrlFile(string file, bool waitForClose = false)
		{
			string url = "";
			string[] lines;

			if (!File.Exists(file))
			{
				MessageBox.Show("Could not find or do not have access to .url file.", "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			lines = File.ReadAllLines(file);
			if (lines.Length < 2)
			{
				MessageBox.Show("Invalid .url file format.", "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			foreach (string l in lines)
			{
				if (l.StartsWith("URL=", StringComparison.InvariantCulture))
				{
					url = l.Substring(4);
					break;
				}
			}

			if (url.Length > 0)
			{
				LaunchBrowser(url);
			}
			else
			{
				MessageBox.Show("Invalid shortcut file format.", "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		static void LaunchWeblocFile(string file, bool waitForClose = false)
		{
			string url = "";
			XmlDocument doc;
			XmlNode node;

			if (!File.Exists(file))
			{
				MessageBox.Show("Could not find or do not have access to .url file.", "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			doc = new XmlDocument();
			try
			{
				doc.Load(file);
			}
			catch (Exception)
			{
				MessageBox.Show("Could not read the .webloc file.", "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			node = doc.DocumentElement.SelectSingleNode("//plist/dict/string");
			if (node == null)
			{
				MessageBox.Show("Unknown or invalid .webloc file format.", "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			url = node.InnerText;
			if (url.Length > 0)
			{
				LaunchBrowser(url);
			}
		}

		static void LaunchBrowser(string url, bool waitForClose = false)
		{
			try
			{
				var urlPreferences = ConfigReader.GetUrlPreferences();
				//Check if we need to replace http with https
                var forceSSL = ConfigurationManager.AppSettings["ForceSSL"];
                if (automaticMode) forceSSL = "no";
                if (string.IsNullOrEmpty(forceSSL)) forceSSL = "no";
                if (url.ToLowerInvariant().StartsWith("http:") && forceSSL != "no")
                {
                    if (forceSSL == "ask")
                    {
                        var sure = MessageBox.Show("This URL user insecure HTTP protocol. Would you like to switch to SSL instead?", "Use HTTPS?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
						switch (sure)
                        {
                            case DialogResult.Cancel:
                                return;
                            case DialogResult.Yes:
                                forceSSL = "yes";
                                break;
                        }
                    }
                    if (forceSSL == "yes") url = "https" + url.Substring(4, url.Length - 4);

                }
				string _url = url;
				Uri uri = new Uri(_url);

                foreach (var preference in urlPreferences)
				{
					var urlPattern = preference.UrlPattern;

                    string pattern;
                    var domain = "";
                    if (urlPattern.StartsWith("/") && urlPattern.EndsWith("/"))
					{
						// The domain from the INI file is a regex..
						domain = uri.Authority + uri.AbsolutePath;
						pattern = urlPattern.Substring(1, urlPattern.Length - 2);
					}
					else
					{
						// We're only checking the domain.
						domain = uri.Authority;

						// Escape the input for regex; the only special character we support is a *
						var regex = Regex.Escape(urlPattern);
						// Unescape * as a wildcard.
						pattern = string.Format("^{0}$", regex.Replace("\\*", ".*"));
					}

                    if (!Regex.IsMatch(domain, pattern)) continue;
					//Check VPN
                    if (preference.ForceVPN || preference.DisallowVPN)
                    {
                        var isVPN = VPNHelper.IsVPNRunning(preference.ForceVPN);//If not running, but should be enforced, the client will be executed
                        if (isVPN && preference.DisallowVPN)
                        {
                            MessageBox.Show($@"Your VPN is currently running. It should be disabled to access {domain}. Please disable your VPN and try again, OR enter this URL directly into your browser of choice.", "VPN enabled", MessageBoxButtons.OK);
							return;
                        }

                        if (!isVPN && preference.ForceVPN)
                        {
                            var sure = MessageBox.Show($@"VPN should be enabled to access {domain}. Browser Selector couldn't start your VPN client, or connection wasn't established within 2 minutes.\n\nWould you still like to attempt to open this URL?\n\nWARNING: this may expose your actual IP address if the VPN is indeed not running.", "VPN not running", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
							if (sure == DialogResult.No) return;
                        }
                    }

                    string loc = preference.Browser.Location;
                    if (string.IsNullOrEmpty(loc))
                    {
                        var editINI = MessageBox.Show($"The location of {preference.Browser.Name} browser is empty. Please check your BrowserSelector.ini file!\n\nWould you like to open it now?", "Invalid browser location", MessageBoxButtons.YesNo, MessageBoxIcon.Stop);
                        if (editINI == DialogResult.No) return;
                        //Open ini in notepad
                        try
                        {
                            Process.Start("notepad.exe", ConfigReader.ConfigPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error starting notepad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        return;
                    }
                    if (loc.IndexOf("{url}") > -1)
                    {
                        loc = loc.Replace("{url}", _url);
                        _url = "";
                    }

                    Process p;
                    if (loc.StartsWith("\"") && loc.IndexOf('"', 2) > -1)
                    {
                        // Assume the quoted item is the executable, while everything
                        // after (the second quote), is part of the command-line arguments.
                        loc = loc.Substring(1);
                        int pos = loc.IndexOf('"');
                        string args = loc.Substring(pos + 1).Trim();
                        loc = loc.Substring(0, pos).Trim();
                        if (loc == "null") return;
                        p = Process.Start(loc, args + " " + _url);
                    }
                    else
                    {
                        // The browser specified in the INI file is a single executable
                        // without any other arguments.
                        // (normal/original behavior)
                        if (loc == "null") return;
                        p = Process.Start(loc, _url);
                    }

                    if (waitForClose)
                    {
                        p.WaitForExit();
                    }

                    return;
                }

				MessageBox.Show($"Unable to find a suitable browser matching {url}.", "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Unable to launch browser\n\n{ex}.", "BrowserSelector", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		static void CreateSampleSettings()
		{
			DialogResult r = DialogResult.Yes;

			if (File.Exists(ConfigReader.ConfigPath))
			{
				r = MessageBox.Show(@"The settings file already exists. Would you like to replace it with the sample file? (The existing file will be saved/renamed.)", "BrowserSelector", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
			}

			if (r == DialogResult.No)
				return;

			ConfigReader.CreateSampleIni();
		}
	}
}
