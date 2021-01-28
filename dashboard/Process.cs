using System;
using System.Management;
using System.Runtime.InteropServices;

namespace dashboard
{
    class Process : System.Diagnostics.Process
    {
        public int pid;
        public string exe;
        public string args;
        public IntPtr hwnd;
        public ManagementObjectSearcher query;

        // GetWindowThreadProcessId
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        public Process GetPID(IntPtr hwnd)
        {
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);

            this.pid = (int)pid;

            return this;
        }

        public Process GetQuery(string exe, string args)
        {
            Resource file = new Resource(exe);
            file.GetRegPath();
            
            string queryString = null;
            queryString = @"SELECT CreationDate, ProcessId FROM Win32_Process WHERE ";
            queryString += ExeCondition(file.name, args);
            queryString += ExeCondition(file.regPath, args, true);

            string ExeCondition(string exe, string args, Boolean seq = false)
            {
                string result = "";

                if (!String.IsNullOrEmpty(exe))
                {
                    result = seq ? " OR " : "";
                    result += "CommandLine LIKE '" + exe + "%" + args + "%'";
                    result += " OR ";
                    result += "CommandLine LIKE '_" + exe + "%" + args + "%'";
                    result += " OR ";
                    result += "Name LIKE '" + exe + "%" + args + "%'";
                }

                return result;
            }

            query = new ManagementObjectSearcher(queryString);

            return this;
        }

        public Process GetQuery(int pid)
        {
            string queryString = @"SELECT ProcessId, CommandLine FROM Win32_Process WHERE ProcessId = " + pid;

            query = new ManagementObjectSearcher(queryString);

            return this;
        }
    }
}
