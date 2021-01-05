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
            int processTimeout = 5;
            IntPtr result = IntPtr.Zero;

            try
            {
                ManagementObjectSearcher mos = new ManagementObjectSearcher(ProcessManager.GetProcessQueryByExe(exe, args));

                foreach (ManagementObject mo in mos.Get())
                {
                    DateTime now = DateTime.Now;
                    DateTime creationDate = ManagementDateTimeConverter.ToDateTime((string)mo["CreationDate"]);
                    double elapsedSeconds = (now - creationDate).TotalSeconds;

                    if (elapsedSeconds >= 0 && elapsedSeconds < processTimeout)
                    {
                        Process p = Process.GetProcessById(Convert.ToInt32(mo["ProcessId"]));
                        result = p.MainWindowHandle;
                    }

                    break;
                }
            }
            catch (Exception exception)
            {
                ResourceManager.WriteLog(exception);
            }

            return result;
        }

        public static IntPtr GetWindowByTitle(string title)
        {
            return FindWindow(null, title);
        }

        public static Dictionary<string, int> GetWindowPosition(string title)
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
    }
}
