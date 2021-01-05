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
                // Get the creation date and PID of every process who has this executable + these arguments as command line
                ManagementObjectSearcher mos = new ManagementObjectSearcher(ProcessManager.GetProcessQueryByExe(exe, args));
                // Create a list to store pairs made of the creation date => the PID of each process
                List<KeyValuePair<DateTime, int>> processes = new List<KeyValuePair<DateTime, int>>();

                // Loop through each query row and add the pairs of creation date => PID to the list created previously
                foreach (ManagementObject mo in mos.Get())
                {
                    DateTime creationDate = ManagementDateTimeConverter.ToDateTime((string)mo["CreationDate"]);
                    int processId = Convert.ToInt32(mo["ProcessId"]);
                    processes.Add(new KeyValuePair<DateTime, int>(creationDate, processId));
                }

                // Sort the list in descendant order so, if there's more than one pair, we can find the most recent one later on
                processes.Sort((pair1, pair2) => DateTime.Compare(pair2.Key, pair1.Key));

                // Access the most recent pair using the first list item's index
                Process p = Process.GetProcessById(processes[0].Value);
                result = p.MainWindowHandle;

                Console.WriteLine(processes[0].Value);

                // It's important to use a KeyValuePair list in this case because we need to, at the same time, make a
                // relation between creation date and PID and also be able to reinforce an order in the list,
                // which is not possible using a dictionary
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
