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

        public EnvironmentManager()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Error;
        }

        public static object GetEnvironment(string environmentName)
        {
            if (File.Exists(configPath))
            {
                try
                {
                    environments = JsonConvert.DeserializeObject(File.ReadAllText(configPath));

                    return environments[environmentName];
                }
                catch (Exception exception)
                {
                    ResourceManager.WriteLog(exception);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static Boolean AddProfileToEnvironment(Profile profile, string environmentName)
        {
            try
            {
                JArray environment = (JArray)EnvironmentManager.GetEnvironment(environmentName);
                environment.Add(JToken.FromObject(profile));
                File.WriteAllText(configPath, EnvironmentManager.environments.ToString());

                return true;
            }
            catch (Exception exception)
            {
                ResourceManager.WriteLog(exception);
                return false;
            }
        }
    }
}
