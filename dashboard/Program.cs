using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace dashboard
{
    class Program
    {
        // EnumWindows
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        // GetWindowThreadProcessId
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out uint processId);

        // Delegate to filter which windows to include 
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private static string getRegName(string fileName)
        {
            object hkcuPath = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\" + fileName, null, null);
            object hklmPath = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\" + fileName, null, null);
            object filePath = hkcuPath ?? hklmPath;

            return filePath == null ? "" : Path.GetFileName((string)filePath);
        }

        static void Main(string[] args)
        {
            Process p = new Process();
            p.StartInfo.FileName = "wt.exe";
            p.StartInfo.Arguments = "-p \"CMatrix Red\"";
            p.Start();

            EnumWindows(delegate (IntPtr hwnd, IntPtr param)
            {
                uint pid = 0;
                GetWindowThreadProcessId(hwnd, out pid);
                Process p = Process.GetProcessById((int)pid);
                string fileName = Path.GetFileName(p.MainModule.FileName);

                if (fileName == "wt.exe" || fileName == getRegName("wt.exe"))
                {
                    Console.WriteLine(hwnd);
                    return false;
                }

                return true;
            }, IntPtr.Zero);
        }
    }
}
