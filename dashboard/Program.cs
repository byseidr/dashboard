using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;

namespace dashboard
{
    class Program
    {
        private static Dictionary<string, int> zPos = new Dictionary<string, int>(){
            {"bottom", 1},
            {"top", 0},
            {"topmost", -1},
            {"notopmost", -2}
        };

        // FindWindow
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        // ShowWindow
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        // SetWindowPos
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        private static string origAttr(string[] profile, int i)
        {
            return profile.Length > i ? profile[i] : "";
        }

        private static T getAttr<T>(string arg, T dft)
        {
            return !String.IsNullOrEmpty(arg) ? (T)Convert.ChangeType(arg, typeof(T)) : dft;
        }

        private static List<object> getProfile(string arg)
        {
            string[] profile = arg.Split(";");

            List<object> result = new List<object>(){
                // Exe + args
                {origAttr(profile, 0)}, {origAttr(profile, 1)},
                // X, Y
                {getAttr(origAttr(profile, 2), 200)}, {getAttr(origAttr(profile, 3), 100)},
                // Width, height
                {getAttr(origAttr(profile, 4), 1000)}, {getAttr(origAttr(profile, 5), 600)},
                // Z order
                {zPos[getAttr(origAttr(profile, 6), "notopmost")]}
            };

            return result;
        }

        private static string getRegPath(string exe)
        {
            object hkcuPath = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\" + exe, null, null);
            object hklmPath = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\" + exe, null, null);
            object filePath = hkcuPath ?? hklmPath;

            return filePath == null ? null : Path.GetFileName((string)filePath);
        }

        private static string getQuery(string exe, string args)
        {
            exe = exe.Replace("\\", "\\\\");

            string query;
            query = @"SELECT ProcessId FROM Win32_Process WHERE ";
            query += getQueryWhere(exe, args);
            query += getQueryWhere(getRegPath(exe), args, true);

            return query;
        }

        private static string getQueryWhere(string exe, string args, Boolean seq = false)
        {
            if (String.IsNullOrEmpty(exe)) return "";

            string where;
            where = seq ? " OR " : "";
            where += "CommandLine LIKE '" + exe + "%" + args + "%'";
            where += " OR ";
            where += "CommandLine LIKE '_" + exe + "%" + args + "%'";
            where += " OR ";
            where += "Name LIKE '" + exe + "%" + args + "%'";

            return where;
        }

        private static Boolean positionWindow(string exe, string args, int x, int y, int cx, int cy, int zPos)
        {
            ManagementObjectSearcher mos = new ManagementObjectSearcher(getQuery(exe, args));

            foreach (ManagementObject mo in mos.Get())
            {
                try
                {
                    Process p = Process.GetProcessById(Convert.ToInt32(mo["ProcessId"]));

                    if (p.MainWindowHandle != IntPtr.Zero)
                    {
                        ShowWindow(p.MainWindowHandle, 1);
                        SetWindowPos(p.MainWindowHandle, zPos, x, y, cx, cy, 0x00);

                        return true;
                    }

                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                foreach (string arg in args)
                {
                    List<object> profile = getProfile(arg);

                    if (!String.IsNullOrEmpty((string)profile[0]))
                    {
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = true;
                        p.StartInfo.FileName = (string)profile[0];
                        if (!String.IsNullOrEmpty((string)profile[1]))
                        {
                            p.StartInfo.Arguments = (string)profile[1];
                        }
                        p.Start();

                        Thread.Sleep(1000);

                        positionWindow(
                            (string)profile[0],
                            (string)profile[1],
                            (int)profile[2],
                            (int)profile[3],
                            (int)profile[4],
                            (int)profile[5],
                            (int)profile[6]
                        );
                    }
                }
            }
        }
    }
}
