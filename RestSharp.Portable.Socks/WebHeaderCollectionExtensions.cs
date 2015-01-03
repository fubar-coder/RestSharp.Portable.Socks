using System;
using System.Collections.Generic;
using System.Text;

namespace RestSharp.Portable.Socks
{
    static class WebHeaderCollectionExtensions
    {
        internal static string[] GetValues(IList<string> values)
        {
            var tmp = new string[values.Count];
            var i = 0;
            foreach (var v in values)
                tmp[i++] = v;
            return tmp;
        }

        public static string[] GetValues(this IDictionary<string, IList<string>> collection, string key)
        {
            IList<string> values;
            if (!collection.TryGetValue(key, out values))
                return new string[0];
            return GetValues(values);
        }

        public static void Add(this IDictionary<string, IList<string>> collection, KeyValuePair<string, string> kvp)
        {
            IList<string> entry;
            if (!collection.TryGetValue(kvp.Key, out entry))
                collection.Add(kvp.Key, new List<string>());
            entry.Add(kvp.Value);
        }
    }
}
