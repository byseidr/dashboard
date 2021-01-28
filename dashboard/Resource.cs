using Microsoft.Win32;
using System;
using System.IO;

namespace dashboard
{
    class Resource
    {
        public string name;
        public string args { get; set; } = "";
        public string regPath { get; set; } = "";

        public static string logPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.config\dashboard\log.txt");

        public Resource(string name)
        {
            this.name = name.Trim().Replace("\\", "\\\\");
        }

        public void GetRegPath()
        {
            object hkcuPath = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\" + this.name, null, null);
            object hklmPath = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\" + this.name, null, null);
            object filePath = hkcuPath ?? hklmPath;

            this.regPath = filePath == null ? null : Path.GetFileName((string)filePath);
        }

        public void SplitName()
        {
            // Filename is not empty and is not already a full path, meaning no arguments would exist
            if (!string.IsNullOrEmpty(this.name) && !File.Exists(this.name) && !Directory.Exists(this.name))
            {
                string filename = this.name;

                if (this.name.StartsWith(@"""") && this.name.Contains(@""" "))
                {
                    filename = this.name.Substring(0, this.name.IndexOf(@"""", 1) + 1);
                }
                else if (!this.name.StartsWith(@"""") && this.name.Contains(' '))
                {
                    filename = this.name.Substring(0, this.name.IndexOf(@" "));
                }

                if (filename != this.name)
                {
                    this.args = this.name.Substring(filename.Length + 1);
                    this.name = filename;
                }
            }
        }

        public static void WriteLog(Exception exception)
        {
            string log = "(" + DateTime.Now.ToString("o") + ") " + exception.GetType().Name + ": " + exception.Message + Environment.NewLine;

            File.AppendAllText(logPath, log);
        }
    }
}
