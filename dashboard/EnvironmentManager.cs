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
            object result = null;

            if (File.Exists(configPath))
            {
                try
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.MissingMemberHandling = MissingMemberHandling.Error;
                    environments = JsonConvert.DeserializeObject(File.ReadAllText(configPath), settings);

                    result = environments[environmentName];
                }
                catch (Exception exception)
                {
                    ResourceManager.WriteLog(exception);
                }
            }

            return result;
        }

        public static Boolean AddProfileToEnvironment(Profile profile, string environmentName)
        {
            Boolean result = false;

            try
            {
                JArray environment = (JArray)EnvironmentManager.GetEnvironment(environmentName);
                environment.Add(JToken.FromObject(profile));
                File.WriteAllText(configPath, EnvironmentManager.environments.ToString());

                result = true;
            }
            catch (Exception exception)
            {
                ResourceManager.WriteLog(exception);
            }

            return result;
        }
    }
}
