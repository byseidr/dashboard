using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        private static void processProfiles(Dictionary<string, dynamic> profile)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.FileName = (string)profile["exe"];
            if (!String.IsNullOrEmpty((string)profile["args"]))
            {
                p.StartInfo.Arguments = (string)profile["args"];
            }
            p.Start();

            Thread.Sleep(1000);

            positionWindow(
                (string)profile["exe"],
                (string)profile["args"],
                (int)profile["x"],
                (int)profile["y"],
                (int)profile["cx"],
                (int)profile["cy"],
                (int)profile["zPos"]
            );
        }

        private static Dictionary<string, dynamic> objToProfile(Dictionary<string, dynamic> profile)
        {
            Dictionary<string, dynamic> result = new Dictionary<string, dynamic>(){
                {"exe", (string)profile["exe"]},
                {"args", (string)profile["args"]},
                {"x", getAttr((string)profile["x"], 200)},
                {"y", getAttr((string)profile["y"], 100)},
                {"cx", getAttr((string)profile["cx"], 1000)},
                {"cy", getAttr((string)profile["cy"], 600)},
                {"zPos", zPos[getAttr((string)profile["zPos"], "notopmost")]}
            };

            return result;
        }

        private static Dictionary<string, dynamic> argToObj(string arg)
        {
            string[] attr = arg.Split(";");

            Dictionary<string, dynamic> result = new Dictionary<string, dynamic>(){
                {"exe", origAttr(attr, 0)},
                {"args", origAttr(attr, 1)},
                {"x", origAttr(attr, 2)},
                {"y", origAttr(attr, 3)},
                {"cx", origAttr(attr, 4)},
                {"cy", origAttr(attr, 5)},
                {"zPos", origAttr(attr, 6)}
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

        private static void positionWindow(string exe, string args, int x, int y, int cx, int cy, int zPos)
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
                    }
                }
                catch (Exception) {}
            }
        }

        public static string[] getArgs(string[] arr)
        {
            return arr.Where((e, i) => i != 0).ToArray();
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "-p":
                        foreach (string arg in getArgs(args))
                        {
                            Dictionary<string, dynamic> profile = objToProfile(argToObj(arg));

                            if (!String.IsNullOrEmpty((string)profile["exe"]))
                            {
                                //Console.WriteLine(profile["exe"]);
                                processProfiles(profile);
                            }
                        }
                        break;
                    case "-e":
                        string envsPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.config\dashboard\environments.json");
                        dynamic envsObj = JsonConvert.DeserializeObject(File.ReadAllText(envsPath));

                        foreach (JObject arg in envsObj[args[1]])
                        {
                            Dictionary<string, dynamic> profile = objToProfile(arg.ToObject<Dictionary<string, dynamic>>());

                            if (!String.IsNullOrEmpty((string)profile["exe"]))
                            {
                                //Console.WriteLine(profile["exe"]);
                                processProfiles(profile);
                            }
                        }
                        break;
                }
            }
        }
    }
}
