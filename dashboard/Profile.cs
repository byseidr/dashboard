using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;

namespace dashboard
{
    class Profile
    {
        private string exe { get; set; }
        private string args { get; set; }
        private int x { get; set; } = 200;
        private int y { get; set; } = 100;
        private int cx { get; set; } = 1000;
        private int cy { get; set; } = 600;
        private int zPos { get; set; } = 0;

        public string Exe { get => exe; set => exe = value; }
        public string Args { get => args; set => args = value; }
        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public int CX { get => cx; set => cx = value; }
        public int CY { get => cy; set => cy = value; }
        public int ZPos { get => zPos; set => zPos = value; }

        public static string[] attrOrder = { "Exe", "Args", "X", "Y", "CX", "CY", "ZPos" };
        public static object[] attrTypes = { typeof(string), typeof(string), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) };
        public static Dictionary<string, int> attrTranslation = new Dictionary<string, int>()
        {
            { "bottom", 1 },
            { "top", 0 },
            { "topmost", -1 },
            { "notopmost", -2 }
        };

        public object this[string propertyName]
        {
            get => typeof(Profile).GetProperty(propertyName).GetValue(this, null);
            set => typeof(Profile).GetProperty(propertyName).SetValue(this, value, null);
        }

        // GetForegroundWindow
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // GetWindowRect
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        // RECT
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // FindWindow
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        // ShowWindow
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        // SetWindowPos
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        public static IntPtr GetHWND()
        {
            return GetForegroundWindow();
        }

        public static IntPtr GetHWND(string exe, string args)
        {
            IntPtr result = IntPtr.Zero;

            try
            {
                Process process = new Process();
                ManagementObjectSearcher mos = process.GetQuery(exe, args).query;

                foreach (ManagementObject mo in mos.Get())
                {
                    Process p = (Process)Process.GetProcessById(Convert.ToInt32(mo["ProcessId"]));
                    result = p.MainWindowHandle;

                    if (result != IntPtr.Zero)
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            catch (Exception exception)
            {
                Resource.WriteLog(exception);
            }

            return result;
        }

        public static IntPtr GetHWND(string title)
        {
            return FindWindow(null, title);
        }

        public static Dictionary<string, int> GetPosition(string title)
        {
            Dictionary<string, int> result = null;

            if (SpinWait.SpinUntil(() => FindWindow(null, title) != IntPtr.Zero, TimeSpan.FromSeconds(30)))
            {
                IntPtr hwnd = FindWindow(null, title);
                RECT rct = new RECT();
                GetWindowRect(hwnd, ref rct);

                result = new Dictionary<string, int>()
                {
                    { "x", rct.Left },
                    { "y", rct.Top },
                    { "cx", (rct.Right - rct.Left) },
                    { "cy", (rct.Bottom - rct.Top) }
                };
            }

            return result;
        }

        public void GetProfile(IntPtr hwnd)
        {
            Process process = new Process();
            int pid = process.GetPID(hwnd).pid;
            ManagementObjectSearcher mos = process.GetQuery(pid).query;

            foreach (ManagementObject mo in mos.Get())
            {
                try
                {
                    Process p = (Process)Process.GetProcessById(Convert.ToInt32(mo["ProcessId"]));
                    Resource file = new Resource((string)mo["CommandLine"]);
                    file.SplitName();
                    Dictionary<string, int> position = GetPosition(p.MainWindowTitle);

                    this.Exe = file.name;
                    this.Args = file.args;
                    this.X = position["x"];
                    this.Y = position["y"];
                    this.CX = position["cx"];
                    this.CY = position["cy"];
                    this.ZPos = 0;
                }
                catch (Exception exception)
                {
                    Resource.WriteLog(exception);
                }
            }
        }

        public void GetProfile(string[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (!String.IsNullOrEmpty(arr[i]))
                {
                    object attrType = attrTypes[i];

                    if (attrTranslation.ContainsKey(arr[i]))
                    {
                        this[attrOrder[i]] = attrTranslation[arr[i]];
                    }
                    else if ((Type)attrType == typeof(int))
                    {
                        this[attrOrder[i]] = Int32.Parse(arr[i]);
                    }
                    else
                    {
                        this[attrOrder[i]] = arr[i];
                    }
                }
            }
        }

        public void GetProfile(Dictionary<string, string> obj)
        {
            foreach (KeyValuePair<string, string> attr in obj)
            {
                if (!String.IsNullOrEmpty(attr.Value))
                {
                    object attrType = attrTypes[Array.IndexOf(attrOrder, attr.Key)];

                    if (attrTranslation.ContainsKey(attr.Value))
                    {
                        this[attr.Key] = attrTranslation[attr.Value];
                    }
                    else if ((Type)attrType == typeof(int))
                    {
                        this[attr.Key] = Int32.Parse(attr.Value);
                    }
                    else
                    {
                        this[attr.Key] = attr.Value;
                    }
                }
            }
        }

        public void GetProfile(string str)
        {
            this.GetProfile(str.Split(";"));
        }

        public static void RunProfile(Profile profile)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.FileName = profile.Exe;
            if (!String.IsNullOrEmpty(profile.Args)) p.StartInfo.Arguments = profile.Args;
            p.Start();

            Thread.Sleep(1000);

            SetPosition(profile.Exe, profile.Args, profile.X, profile.Y, profile.CX, profile.CY, profile.ZPos);
        }

        public static void SetPosition(string exe, string args, int x, int y, int cx, int cy, int zPos)
        {
            IntPtr hwnd = GetHWND(exe, args);

            if (hwnd != IntPtr.Zero)
            {
                ShowWindow(hwnd, 1);
                SetWindowPos(hwnd, zPos, x, y, cx, cy, 0x00);
            }
        }
    }
}
