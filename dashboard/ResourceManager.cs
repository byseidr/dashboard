using System;
using System.IO;

namespace dashboard
{
    class ResourceManager
    {
        public static string logPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.config\dashboard\log.json");

        public static void WriteLog(Exception exception)
        {
            File.WriteAllText(logPath, exception.GetType().Name + ": " + exception.Message);
        }
    }
}
