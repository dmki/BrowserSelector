using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace BrowserSelector
{
	static class ConfigReader
	{
        /// <summary>
        /// Config lives in the same folder as the EXE, name "BrowserSelector.ini".
        /// </summary>
        public static string ConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BrowserSelector.ini");

        internal static IEnumerable<UrlPreference> GetUrlPreferences()
        {
			if (!File.Exists(ConfigPath))
				throw new InvalidOperationException($"The config file was not found:\r\n{ConfigPath}\r\n");

			// Poor mans INI file reading... Skip comment lines (TODO: support comments on other lines).
			var configLines =
				File.ReadAllLines(ConfigPath)
				.Select(l => l.Trim())
				.Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith(";") && !l.StartsWith("#"));

			// Read the browsers section into a dictionary.
			var browsers = GetConfig(configLines, "browsers")
				.Select(SplitConfig)
				.Select(kvp => new Browser { Name = kvp.Key, Location = kvp.Value })
				.ToDictionary(b => b.Name);

			// If there weren't any at all, force IE in there (nobody should create a config file like this!).
			if (!browsers.Any())
				browsers.Add("ie", new Browser { Name = "ie", Location = @"iexplore.exe ""{0}""" });

			// Read the url preferences, and add a catchall ("*") for the first browser.
			var urls = GetConfig(configLines, "urls")
				.Select(SplitConfig)
				.Select(kvp => new UrlPreference { UrlPattern = kvp.Key, Browser = browsers.ContainsKey(kvp.Value) ? browsers[kvp.Value] : null })
				.Union(new[] { new UrlPreference { UrlPattern = "*", Browser = browsers.FirstOrDefault().Value } }) // Add in a catchall that uses the first browser
				.Where(up => up.Browser != null).ToList();

			//Read VPN preferences
            var vpns = GetConfig(configLines, "vpn").Select(SplitConfig);
            foreach (var vpn in vpns)
            {//These masks can be more vague than actual urls, e.g. *.ie
                var vpnValue = vpn.Value.ToLowerInvariant();
                List<UrlPreference> affectedUrls;
                if (vpn.Key.Contains("*"))
                {
                    var regex = Regex.Escape(vpn.Key);
                    // Unescape * as a wildcard.
                    var pattern = string.Format("^{0}$", regex.Replace("\\*", ".*"));
                    affectedUrls = (from q in urls where Regex.IsMatch(q.UrlPattern, pattern) select q).ToList();

                } else //This is a normal url
                {
                    affectedUrls = (from q in urls where q.UrlPattern == vpn.Key select q).ToList();
                }

                if (!affectedUrls.Any())
                {
                    //Create new url for this website with default browser
                    UrlPreference up = new UrlPreference {UrlPattern = vpn.Key, Browser = browsers.First().Value, ForceVPN = (vpnValue == "force"), DisallowVPN = (vpnValue == "disable")};
					urls.Insert(urls.Count - 1, up);
                    //urls.Add(up);
                    continue;
                }
				//Now specify VPN values for affected URLs
                foreach (var url in affectedUrls)
                {
                    if (vpnValue == "force") url.ForceVPN = true;
                    if (vpnValue == "disable") url.DisallowVPN = true;
                    //var z = (from q in urls where q.UrlPattern == url.UrlPattern select q).First();
                    //z.ForceVPN = url.ForceVPN;
                    //z.DisallowVPN = url.DisallowVPN;
                }
            }

			return urls;
		}

        internal static IEnumerable<Browser> GetBrowsers()
        {
            if (!File.Exists(ConfigPath)) throw new InvalidOperationException($"The config file was not found:\r\n{ConfigPath}\r\n");
            var configLines =
                File.ReadAllLines(ConfigPath)
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith(";") && !l.StartsWith("#"));
            return GetConfig(configLines, "browsers")
                .Select(SplitConfig)
                .Select(kvp => new Browser {Name = kvp.Key, Location = kvp.Value});
        }

		static IEnumerable<string> GetConfig(IEnumerable<string> configLines, string configName)
		{
			// Read everything from [configName] up to the next [section].
			return configLines
				.SkipWhile(l => !l.StartsWith($"[{configName}]", StringComparison.OrdinalIgnoreCase))
				.Skip(1)
				.TakeWhile(l => !l.StartsWith("[", StringComparison.OrdinalIgnoreCase))
				.Where(l => l.Contains('='));
		}

		/// <summary>
		/// Splits a line on the first '=' (poor INI parsing).
		/// </summary>
		static KeyValuePair<string, string> SplitConfig(string configLine)
		{
			var parts = configLine.Split(new[] { '=' }, 2);
			return new KeyValuePair<string, string>(parts[0].Trim(), parts[1].Trim());
		}

		public static void CreateSampleIni()
		{
            var assembly = Assembly.GetExecutingAssembly();
			//stream = assembly.GetManifestResourceStream("DanTup.BrowserSelector.BrowserSelector.ini");
			var stream = assembly.GetManifestResourceStream(assembly.GetManifestResourceNames()[0]);
			if (stream == null)
			{
				return;
			}

			var result = new StringBuilder();

			using (StreamReader reader = new StreamReader(stream))
			{
				result.Append(reader.ReadToEnd());
				reader.Close();
			}

            if (result.Length == 0) return;
            if (File.Exists(ConfigPath))
            {
                string newName = GetBackupFileName(ConfigPath);
                File.Move(ConfigPath, newName);
            }

            File.WriteAllText(ConfigPath, result.ToString());
        }

		static string GetBackupFileName(string fileName)
		{
			string newName;
            int index = 0;

			var fname = Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName);
			var fext = Path.GetExtension(fileName);

			do
			{
				newName = string.Format("{0}.{1:0000}{2}", fname, ++index, fext);
			} while (File.Exists(newName));

			return newName;
		}
	}

	class Browser
	{
		public string Name { get; set; }
		public string Location { get; set; }
	}

	class UrlPreference
	{
		public string UrlPattern { get; set; }
		public Browser Browser { get; set; }
        public bool ForceVPN { get; set; } = false;
        public bool DisallowVPN { get; set; } = false;
    }
}
