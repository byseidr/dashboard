using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dashboard
{
    class Program
    {
        static void Main(string[] args)
        {
            string option = args[0];
            args = args.Where((e, i) => i != 0).ToArray();

            if (args.Length > 0)
            {
                switch (option)
                {
                    case "-p":
                        DoProfile(args);
                        break;
                    case "-e":
                        DoEnvironment(args);
                        break;
                    case "-a":
                        DoAdd(args);
                        break;
                }
            }
        }

        private static void DoProfile(string[] args)
        {
            foreach (string arg in args)
            {
                Profile profile = new Profile(arg.Split(";"));

                if (!String.IsNullOrEmpty(profile.Exe))
                {
                    //Console.WriteLine(profile["exe"]);
                    ProfileManager.RunProfile(profile);
                }
            }
        }

        private static void DoEnvironment(string[] args)
        {
            dynamic envsObj = FileManager.ParseJSON(@"%USERPROFILE%\.config\dashboard\environments.json");

            foreach (JObject arg in envsObj[args[0]])
            {
                Profile profile = new Profile(arg.ToObject<Dictionary<string, string>>());

                if (!String.IsNullOrEmpty(profile.Exe))
                {
                    //Console.WriteLine(profile["exe"]);
                    ProfileManager.RunProfile(profile);
                }
            }
        }

        private static void DoAdd(string[] args)
        {
            IntPtr hwnd = WindowManager.GetActiveWindow();
            Dictionary<string, string> profileObj = ProfileManager.GetProfileFromHWND(hwnd);
            Profile profile = new Profile(profileObj);

            dynamic envsObj = FileManager.ParseJSON(@"%USERPROFILE%\.config\dashboard\environments.json");
            JArray environment = (JArray)envsObj[args[0]];
            environment.Add(JToken.FromObject(profile));

            Console.WriteLine(envsObj.ToString());
        }
    }
}
