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
            dynamic environment = EnvironmentManager.GetEnvironment(args[0]);

            foreach (JObject arg in environment)
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
            string environment = args[0];
            string source = args[1];

            if (environment.Length > 0 && source.Length > 0)
            {
                IntPtr hwnd;

                if (source == "active")
                {
                    hwnd = WindowManager.GetActiveWindow();
                }
                else
                {
                    hwnd = WindowManager.GetWindowByTitle(source);
                }


                Dictionary<string, string> profileObj = ProfileManager.GetProfileFromHWND(hwnd);
                Profile profile = new Profile(profileObj);

                EnvironmentManager.AddProfileToEnvironment(profile, environment);
            }
        }
    }
}
