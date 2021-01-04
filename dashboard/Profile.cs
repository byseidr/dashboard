using System;
using System.Collections.Generic;

namespace dashboard
{
    class Profile
    {
        private string exe { get; set; }
        private string args { get; set; }
        private int x { get; set; } = 200;
        private int y { get; set; } = 100;
        private int cx { get; set; } = 1000;
        private int cy { get; set; } = 600;
        private int zPos { get; set; } = 0;

        public string Exe { get => exe; set => exe = value; }
        public string Args { get => args; set => args = value; }
        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public int CX { get => cx; set => cx = value; }
        public int CY { get => cy; set => cy = value; }
        public int ZPos { get => zPos; set => zPos = value; }

        public static string[] attrOrder = { "Exe", "Args", "X", "Y", "CX", "CY", "ZPos" };
        public static object[] attrTypes = { typeof(string), typeof(string), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) };
        public static Dictionary<string, int> attrTranslation = new Dictionary<string, int>()
        {
            { "bottom", 1 },
            { "top", 0 },
            { "topmost", -1 },
            { "notopmost", -2 }
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
                if (!String.IsNullOrEmpty(arr[i]))
                {
                    object attrType = attrTypes[i];

                    if (attrTranslation.ContainsKey(arr[i]))
                    {
                        this[attrOrder[i]] = attrTranslation[arr[i]];
                    }
                    else if ((Type)attrType == typeof(int))
                    {
                        this[attrOrder[i]] = Int32.Parse(arr[i]);
                    }
                    else
                    {
                        this[attrOrder[i]] = arr[i];
                    }
                }
            }
        }

        public void FromObj(Dictionary<string, string> obj)
        {
            foreach (KeyValuePair<string, string> attr in obj)
            {
                if (!String.IsNullOrEmpty(attr.Value))
                {
                    object attrType = attrTypes[Array.IndexOf(attrOrder, attr.Key)];

                    if (attrTranslation.ContainsKey(attr.Value))
                    {
                        this[attr.Key] = attrTranslation[attr.Value];
                    }
                    else if ((Type)attrType == typeof(int))
                    {
                        this[attr.Key] = Int32.Parse(attr.Value);
                    }
                    else
                    {
                        this[attr.Key] = attr.Value;
                    }
                }
            }
        }
    }
}
