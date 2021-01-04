using System;
using System.Collections.Generic;

namespace dashboard
{
    class Profile
    {
        public string exe { get; set; }
        public string args { get; set; }
        public string x { get; set; } = "200";
        public string y { get; set; } = "100";
        public string cx { get; set; } = "1000";
        public string cy { get; set; } = "600";
        public string zPos { get; set; } = "0";

        public string Exe { get => exe; }
        public string Args { get => args; }
        public int X { get => Int32.Parse(x); }
        public int Y { get => Int32.Parse(y); }
        public int CX { get => Int32.Parse(cx); }
        public int CY { get => Int32.Parse(cy); }
        public int ZPos { get => Int32.Parse(zPos); }

        public static string[] attrOrder = { "exe", "args", "x", "y", "cx", "cy", "zPos" };
        public static Dictionary<string, string> attrTranslation = new Dictionary<string, string>()
        {
            { "bottom", "1" },
            { "top", "0" },
            { "topmost", "-1" },
            { "notopmost", "-2" }
        };

        public object this[string propertyName]
        {
            get => typeof(Profile).GetProperty(propertyName).GetValue(this, null);
            set => typeof(Profile).GetProperty(propertyName).SetValue(this, value, null);
        }

        public Profile(string[] arr)
        {
            this.FromArr(arr);
        }

        public Profile(Dictionary<string, string> obj)
        {
            this.FromObj(obj);
        }

        public void FromArr(string[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if(!String.IsNullOrEmpty(arr[i]))
                {
                    this[attrOrder[i]] = attrTranslation.ContainsKey(arr[i]) ? attrTranslation[arr[i]] : arr[i];
                }
            }
        }

        public void FromObj(Dictionary<string, string> obj)
        {
            foreach (KeyValuePair<string, string> attr in obj)
            {
                if (!String.IsNullOrEmpty(attr.Value))
                {
                    this[attr.Key] = attrTranslation.ContainsKey(attr.Value) ? attrTranslation[attr.Value] : attr.Value;
                }
            }
        }
    }
}
