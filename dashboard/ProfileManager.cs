﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;

namespace dashboard
{
    class ProfileManager
    {
        // FindWindow
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        // ShowWindow
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        // SetWindowPos
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        public static Dictionary<string, string> GetProfileFromHWND(IntPtr hwnd)
        {
            int pid = ProcessManager.GetProcessByHWND(hwnd);
            string query = ProcessManager.GetProcessQueryByPID(pid);

            ManagementObjectSearcher mos = new ManagementObjectSearcher(query);

            foreach (ManagementObject mo in mos.Get())
            {
                try
                {
                    Process p = Process.GetProcessById(Convert.ToInt32(mo["ProcessId"]));
                    File file = new File((string)mo["CommandLine"]);
                    file.SplitName();
                    Dictionary<string, int> position = WindowManager.GetWindowPosition(p.MainWindowTitle);

                    return new Dictionary<string, string>()
                    {
                        { "exe", file.name },
                        { "args", file.args },
                        { "x", position["x"].ToString() },
                        { "y", position["y"].ToString() },
                        { "cx", position["cx"].ToString() },
                        { "cy", position["cy"].ToString() },
                        { "zPos", "" }
                    };
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return null;
        }

        public static void RunProfile(Profile profile)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.FileName = profile.Exe;
            if (!String.IsNullOrEmpty(profile.Args)) p.StartInfo.Arguments = profile.Args;
            p.Start();

            Thread.Sleep(1000);

            PositionWindow(profile.Exe, profile.Args, profile.X, profile.Y, profile.CX, profile.CY, profile.ZPos);
        }

        private static void PositionWindow(string exe, string args, int x, int y, int cx, int cy, int zPos)
        {
            IntPtr hwnd = WindowManager.GetWindowByExe(exe, args);
            if (hwnd != IntPtr.Zero)
            {
                ShowWindow(hwnd, 1);
                SetWindowPos(hwnd, zPos, x, y, cx, cy, 0x00);
            }
        }
    }
}