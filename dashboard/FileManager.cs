using Newtonsoft.Json;
using System;

namespace dashboard
{
    class FileManager
    {
        public static object ParseJSON(string file)
        {
            string envsPath = Environment.ExpandEnvironmentVariables(file);
            return JsonConvert.DeserializeObject(System.IO.File.ReadAllText(envsPath));
        }
    }
}
