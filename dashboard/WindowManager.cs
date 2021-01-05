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

        public static IntPtr GetActiveWindow()
        {
            return GetForegroundWindow();
        }

        public static IntPtr GetWindowByExe(string exe, string args)
        {
            IntPtr result = IntPtr.Zero;

            try
            {
                ManagementObjectSearcher mos = new ManagementObjectSearcher(ProcessManager.GetProcessQueryByExe(exe, args));
                
                foreach (ManagementObject mo in mos.Get())
                {
                    Process p = Process.GetProcessById(Convert.ToInt32(mo["ProcessId"]));
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

        public static void PositionWindow(string exe, string args, int x, int y, int cx, int cy, int zPos)
        {
            IntPtr hwnd = GetWindowByExe(exe, args);

            if (hwnd != IntPtr.Zero)
            {
                ShowWindow(hwnd, 1);
                SetWindowPos(hwnd, zPos, x, y, cx, cy, 0x00);
            }
        }
    }
}
