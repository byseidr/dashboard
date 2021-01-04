﻿using Newtonsoft.Json.Linq;
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

        private static Boolean DoProfile(string[] args)
        {
            try
            {
                foreach (string arg in args)
                {
                    Profile profile = new Profile(arg.Split(";"));

                    if (!String.IsNullOrEmpty(profile.Exe))
                    {
                        //Console.WriteLine(profile["exe"]);
                        ProfileManager.RunProfile(profile);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return false;
            }
            catch (Exception exception)
            {
                ResourceManager.WriteLog(exception);
                return false;
            }
        }

        private static Boolean DoEnvironment(string[] args)
        {
            try
            {
                dynamic environment = EnvironmentManager.GetEnvironment(args[0]);

                foreach (JObject arg in environment)
                {
                    Profile profile = new Profile(arg.ToObject<Dictionary<string, string>>());

                    if (!String.IsNullOrEmpty(profile.Exe))
                    {
                        //Console.WriteLine(profile["exe"]);
                        ProfileManager.RunProfile(profile);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return false;
            }
            catch (Exception exception)
            {
                ResourceManager.WriteLog(exception);
                return false;
            }
        }

        private static Boolean DoAdd(string[] args)
        {
            try
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


                    if (hwnd != IntPtr.Zero)
                    {
                        Dictionary<string, string> profileObj = ProfileManager.GetProfileFromHWND(hwnd);
                        Profile profile = new Profile(profileObj);

                        EnvironmentManager.AddProfileToEnvironment(profile, environment);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                ResourceManager.WriteLog(exception);
                return false;
            }
        }
    }
}
