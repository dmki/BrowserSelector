using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrowserSelector;

namespace DanTup.BrowserSelector
{
    public partial class frmNag : Form
    {
        private bool _loaded = false;
        public frmNag()
        {
            InitializeComponent();
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cmdRegBrowser_Click(object sender, EventArgs e)
        {
            RegistrySettings.RegisterBrowser();
            Close();
        }

        private void cmdUnreg_Click(object sender, EventArgs e)
        {
            RegistrySettings.UnregisterBrowser();
            Close();
        }
        [STAThread]
        private void cmdCheckSettings_Click(object sender, EventArgs e)
        {
            var prefs = ConfigReader.GetBrowsers();
            foreach (var pref in prefs)
            {
                var path = pref.Location.ToLowerInvariant();
                if (string.IsNullOrEmpty(path))
                {
                    MessageBox.Show($"Empty executable path for browser {pref.Name}! Please edit this item manually!", "Empty item", MessageBoxButtons.OK);
                    continue;
                }
                if (path.Contains(@"c:\") && path.Contains(".exe"))
                {
                    int beginning = path.IndexOf(@"c:\", StringComparison.Ordinal);
                    int end = path.IndexOf(".exe", beginning + 1);
                    var pathLen = end + 4 - beginning;
                    var pathName = pref.Location.Substring(beginning, pathLen);
                    if (File.Exists(pathName)) continue;
                    var action = MessageBox.Show($"The path \"{pathName}\" for browser {pref.Name} is invalid. Shall we search for a better executable?\nPress Yes to search for file, No if you want to edit BrowserSelector.ini yourself, and Cancel to stop verification procedure", "Invalid path", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                    if (action == DialogResult.Cancel) return;
                    if (action == DialogResult.No) continue;
                    //Modify INI - search for new exe file, and replace the path in INI with new path
                    var fName = Path.GetFileName(pathName);
                    //Find this file
                    var test = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    string foundFile = GetFile(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), fName);
                    if (string.IsNullOrEmpty(foundFile)) foundFile = GetFile(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), fName);
                    if (string.IsNullOrEmpty(foundFile))
                    {
                        action = MessageBox.Show($"Couldn't find the file {fName} in either Program Files or Program Files(x86). Would you like to select it manually?", "File not found, still", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                        if (action == DialogResult.No) continue;
                        //Open file selection
                        Thread t = new Thread((ThreadStart) (() =>
                        {
                            var ofd = new OpenFileDialog();
                            ofd.Filter = $"{fName}|{fName}";
                            ofd.CheckPathExists = true;
                            ofd.InitialDirectory = @"c:\";
                            ofd.FileName = "";
                            ofd.FilterIndex = 0;
                            action = ofd.ShowDialog();
                            if (action == DialogResult.Cancel) return;
                            foundFile = ofd.FileName;
                        }));
                        
                        t.SetApartmentState(ApartmentState.STA);
                        t.Start();
                        t.Join();
                        
                        if (string.IsNullOrEmpty(foundFile) || !File.Exists(foundFile)) continue;
                    } 
                    //Replace
                    if (!string.IsNullOrEmpty(foundFile) && File.Exists(foundFile)){
                        var iniFileData = File.ReadAllText(ConfigReader.ConfigPath).Replace(pathName, foundFile);
                        File.WriteAllText(ConfigReader.ConfigPath, iniFileData);
                    }
                }
            }

            MessageBox.Show("All browser paths were verified!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal string GetFile(string path, string searchPattern)
        {
            List<string> fileList = new List<string>();
            EnumerateFiles(path, searchPattern, ref fileList, true);
            if (fileList.Any()) return fileList.First();
            return "";
        }

        internal void EnumerateFiles(string path, string searchPattern, ref List<string> fileList, bool stopOnFirst = false)
        {
            var topDirs = Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly).ToList();
            //First - find files in this directory
            fileList.AddRange(Directory.EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly));
            if (stopOnFirst && fileList.Any()) return;
            //Go through each directory under it
            foreach (var dir in topDirs)
            {
                try
                {
                    EnumerateFiles(dir, searchPattern, ref fileList);//recursion
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

        }

        private void frmNag_Load(object sender, EventArgs e)
        {
            chDebug.Checked = RegHelper.GetSettingBool("Debug");
            if (chDebug.Checked) MessageBox.Show("Debug mode is enabled. This will cause BrowserSelector to display message box with command line parameters before performing an action.", "Debug mode enabled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _loaded = true;
        }

        private void chDebug_CheckedChanged(object sender, EventArgs e)
        {
            if (!_loaded) return;
            RegHelper.SaveSetting("Debug", chDebug.Checked);
        }
    }
}
