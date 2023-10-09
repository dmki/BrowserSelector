﻿using System;
using System.Diagnostics;
using System.Windows.Forms;

//using Shell;

namespace BrowserSelector
{
    /// <summary>
    /// Class for invoking commands in Windows
    /// 2014. DMKI
    /// </summary>
    class Command
    {
        private ProcessPriorityClass _priority = ProcessPriorityClass.Normal;
        private int _expectCode = 0;
        private string _Line = "";

        public string Name { get; set; }
        public string Arguments { get; set; }
        public string WorkingDir { get; set; }
        public ProcessWindowStyle UI { get; set; }
        public bool WaitForExit { get; set; }
        public int Timeout { get; set; } //Only if WaitForExit is True
        public string UserName { get; set; }
        public System.Security.SecureString Password { get; set; }
        public string Domain { get; set; }
        public bool Async { get; set; }//Placed for future compatibility with .NET 5. Doesn't work now.
        public bool Run {get; set; }
        public bool Shell {get; set; }
        public bool Verbose { get; set; }
        public bool RedirectStandardOutput;
        public string StandardOutput { get; private set; }//Text variable used to hold output from process

        public string Line 
        {
            get
            {
                return _Line;
            }
            set
            {
                //Check for system variables
                _Line = Environment.ExpandEnvironmentVariables(value);
            }
        }

        public int ExpectCode { 
            get
            {
                return _expectCode;
            }
            set
            {
                _expectCode = value;
                WaitForExit=true;
            }
        }

        public ProcessPriorityClass Priority
        {
            get
            {
                return _priority;
            }
            set
            { 
                if (value == ProcessPriorityClass.RealTime) value = ProcessPriorityClass.High;
                _priority = value;
            }
        }
        public Command()
        {
            //Set some defaults
            UI = ProcessWindowStyle.Hidden;
            UserName = "";
            ExpectCode = -1;
        }
        public int Execute()
        {//Perform desired action, return True on success
            ProcessStartInfo startInfo = new ProcessStartInfo(_Line, Arguments);
            startInfo.CreateNoWindow = (UI== ProcessWindowStyle.Hidden);//So, if hidden it is, no window will be created at the first place
            startInfo.WindowStyle = UI;
            startInfo.UseShellExecute = Shell;
            
            if (RedirectStandardOutput)
            {
                startInfo.RedirectStandardOutput = true;
                WaitForExit = true;
                StandardOutput = String.Empty;
            }
            if (UserName.Length > 0)
            {
                startInfo.UserName = UserName;
                startInfo.Domain = Domain;
                startInfo.Password = Password;
            }
            if (WorkingDir != null) startInfo.WorkingDirectory = WorkingDir;
            Process thisProcess;
            try
            {
               thisProcess = Process.Start(startInfo);
            }
            catch (Exception)
            {
                return -1;
            }
            //Set priority?
            if (thisProcess == null) return 0;
            if (thisProcess.Handle != null) thisProcess.PriorityClass = _priority;
            //WaitForExit?
            if (WaitForExit)
            {
                if (Timeout > 0)
                {
                    thisProcess.WaitForExit(Timeout * 1000);
                }
                else
                {
                    thisProcess.WaitForExit();
                }
                //Returned anything?
                if (RedirectStandardOutput)
                {
                    StandardOutput = thisProcess.StandardOutput.ReadToEnd();
                }
                //Do we expect any exit code?
                if (ExpectCode > -1)
                {
                    var returnCode = thisProcess.ExitCode;
                    //Do we have to do something on fail?
                   if (returnCode != ExpectCode)
                    {
                        MessageBox.Show(
                            $"Process {Name} exited with error code {returnCode} which differs from expected code {ExpectCode}");
                    }
                }
			
			
            }

            return thisProcess.Id;

        }

    }
}