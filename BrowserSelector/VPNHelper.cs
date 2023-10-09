using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrowserSelector
{
    internal static class VPNHelper
    {
        public static bool IsVPNRunning(bool runVPN = false)
        {
            //Check the network adapter
            var vpnAdapterName = ConfigurationManager.AppSettings["VPNInterface"];
            if (string.IsNullOrEmpty(vpnAdapterName)) return false;
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                var inface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(x => x.Name == vpnAdapterName);
                if (inface == null) return false;
                if (inface.OperationalStatus == OperationalStatus.Up) return true;
                if (runVPN)
                {
                    var vpnExe = Common.GetSetting("VPNExecutable");
                    if (string.IsNullOrEmpty(vpnExe)) return false;
                    if (!File.Exists(vpnExe)) return false;
                    //We execute the VPN client and wait for 2 minutes to turn on the interface
                    var cmd = new Command();
                    cmd.Line = vpnExe;
                    var procID = cmd.Execute();
                    if (procID < 1)
                    {
                        MessageBox.Show("Could not execute the VPN client.", "Browser Selector", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    var timeExpires = DateTime.Now.AddMinutes(2);
                    while (DateTime.Now < timeExpires)
                    {
                        Thread.Sleep(1000);
                        if (inface.OperationalStatus == OperationalStatus.Up)
                        {
                            Thread.Sleep(2500);
                            return true;
                        }
                        inface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(x => x.Name == vpnAdapterName);
                    }

                    return false;
                }
                //foreach (NetworkInterface Interface in interfaces)
                //{
                //    // This is the OpenVPN driver for windows. 
                //    if ((Interface.Name == vpnAdapterName) && Interface.OperationalStatus == OperationalStatus.Up)
                //    {
                //        return true;
                //    }
                //}
            }
            return false;
        }
    }
}
