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

            string query;
            query = @"SELECT ProcessId FROM Win32_Process WHERE ";
            query += ExeCondition(exe, args);
            query += ExeCondition(file.regPath, args, true);

            string ExeCondition(string exe, string args, Boolean seq = false)
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

            return query;
        }

        public static string GetProcessQueryByPID(int pid)
        {
            string query = @"SELECT ProcessId, CommandLine FROM Win32_Process WHERE ProcessId = " + pid;

            return query;
        }
    }
}
