using System;
using System.IO;

namespace dashboard
{
    class ResourceManager
    {
        public static string logPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.config\dashboard\log.txt");

        public static void WriteLog(Exception exception)
        {
            string log = "(" + DateTime.Now.ToString("o") + ") " + exception.GetType().Name + ": " + exception.Message + Environment.NewLine;

            File.AppendAllText(logPath, log);
        }
    }
}
