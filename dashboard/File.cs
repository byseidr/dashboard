using Microsoft.Win32;
using System.IO;

namespace dashboard
{
    class File
    {
        public string name;
        public string args { get; set; } = "";
        public string regPath { get; set; } = "";

        public File(string name)
        {
            this.name = name.Trim();
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
            if (!string.IsNullOrEmpty(this.name) && !System.IO.File.Exists(this.name) && !Directory.Exists(this.name))
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
    }
}
