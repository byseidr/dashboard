using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace dashboard
{
    class EnvironmentManager
    {
        public static dynamic environments;
        public static string configPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.config\dashboard\environments.json");

        public static object GetEnvironment(string environmentName)
        {
            string envsPath = configPath;
            environments = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(envsPath));
            return environments[environmentName];
        }

        public static void AddProfileToEnvironment(Profile profile, string environmentName)
        {
            JArray environment = (JArray)EnvironmentManager.GetEnvironment(environmentName);
            environment.Add(JToken.FromObject(profile));
            File.WriteAllText(configPath, EnvironmentManager.environments.ToString());
        }
    }
}
