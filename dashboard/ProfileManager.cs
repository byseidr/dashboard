using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;

namespace dashboard
{
    class ProfileManager
    {
        public static Dictionary<string, string> GetProfileFromHWND(IntPtr hwnd)
        {
            int pid = ProcessManager.GetProcessByHWND(hwnd);
            string query = ProcessManager.GetProcessQueryByPID(pid);
            Dictionary<string, string> result = null;

            ManagementObjectSearcher mos = new ManagementObjectSearcher(query);

            foreach (ManagementObject mo in mos.Get())
            {
                try
                {
                    Process p = Process.GetProcessById(Convert.ToInt32(mo["ProcessId"]));
                    Resource file = new Resource((string)mo["CommandLine"]);
                    file.SplitName();
                    Dictionary<string, int> position = WindowManager.GetWindowPosition(p.MainWindowTitle);

                    result = new Dictionary<string, string>()
                    {
                        { "Exe", file.name },
                        { "Args", file.args },
                        { "X", position["x"].ToString() },
                        { "Y", position["y"].ToString() },
                        { "CX", position["cx"].ToString() },
                        { "CY", position["cy"].ToString() },
                        { "ZPos", "" }
                    };
                }
                catch (Exception exception)
                {
                    ResourceManager.WriteLog(exception);
                }
            }

            return result;
        }

        public static void RunProfile(Profile profile)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.FileName = profile.Exe;
            if (!String.IsNullOrEmpty(profile.Args)) p.StartInfo.Arguments = profile.Args;
            p.Start();

            Thread.Sleep(1000);

            WindowManager.PositionWindow(profile.Exe, profile.Args, profile.X, profile.Y, profile.CX, profile.CY, profile.ZPos);
        }
    }
}
