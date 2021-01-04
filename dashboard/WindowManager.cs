using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;

namespace dashboard
{
    class WindowManager
    {
        // GetForegroundWindow
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // FindWindow
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

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

        public static IntPtr GetActiveWindow()
        {
            return GetForegroundWindow();
        }

        public static IntPtr GetWindowByExe(string exe, string args)
        {
            ManagementObjectSearcher mos = new ManagementObjectSearcher(ProcessManager.GetProcessQueryByExe(exe, args));

            foreach (ManagementObject mo in mos.Get())
            {
                try
                {
                    Process p = Process.GetProcessById(Convert.ToInt32(mo["ProcessId"]));
                    return p.MainWindowHandle;
                }
                catch (Exception)
                {
                    return IntPtr.Zero;
                }
            }

            return IntPtr.Zero;
        }

        public static IntPtr GetWindowByTitle(string title)
        {
            return FindWindow(null, title);
        }

        public static Dictionary<string, int> GetWindowPosition(string title)
        {
            if (SpinWait.SpinUntil(() => FindWindow(null, title) != IntPtr.Zero, TimeSpan.FromSeconds(30)))
            {
                IntPtr hwnd = FindWindow(null, title);
                RECT rct = new RECT();
                GetWindowRect(hwnd, ref rct);

                return new Dictionary<string, int>()
                {
                    { "x", rct.Left },
                    { "y", rct.Top },
                    { "cx", (rct.Right - rct.Left) },
                    { "cy", (rct.Bottom - rct.Top) }
                };
            }
            else
            {
                return null;
            }
        }
    }
}
