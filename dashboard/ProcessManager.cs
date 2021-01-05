using System;
using System.Runtime.InteropServices;

namespace dashboard
{
    class ProcessManager
    {
        // GetWindowThreadProcessId
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        public static int GetProcessByHWND(IntPtr hwnd)
        {
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);

            return (int)pid;
        }

        public static string GetProcessQueryByExe(string exe, string args)
        {
            exe = exe.Replace("\\", "\\\\");
            Resource file = new Resource(exe);
            file.GetRegPath();
            
            string result = null;
            result = @"SELECT CreationDate, ProcessId FROM Win32_Process WHERE ";
            result += ExeCondition(exe, args);
            result += ExeCondition(file.regPath, args, true);

            string ExeCondition(string exe, string args, Boolean seq = false)
            {
                string localResult = "";

                if (!String.IsNullOrEmpty(exe))
                {
                    localResult = seq ? " OR " : "";
                    localResult += "CommandLine LIKE '" + exe + "%" + args + "%'";
                    localResult += " OR ";
                    localResult += "CommandLine LIKE '_" + exe + "%" + args + "%'";
                    localResult += " OR ";
                    localResult += "Name LIKE '" + exe + "%" + args + "%'";
                }

                return localResult;
            }

            return result;
        }

        public static string GetProcessQueryByPID(int pid)
        {
            string result = @"SELECT ProcessId, CommandLine FROM Win32_Process WHERE ProcessId = " + pid;

            return result;
        }
    }
}
