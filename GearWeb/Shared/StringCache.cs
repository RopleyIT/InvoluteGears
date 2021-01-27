using System;
using System.Collections.Generic;
using System.Threading;

namespace GearWeb.Shared
{
    public class StringCache
    {
        private readonly Dictionary<string, string> cache
            = new Dictionary<string, string>();
        private int id = 0;

        private string UniqueId()
        {
            int v = Interlocked.Increment(ref id);
            char[] uidChars = new char[7];
            for (int i = 0; i < uidChars.Length; i++)
            {
                v = Math.DivRem(v, 24, out int rem);
                uidChars[i] = "abcdefghjklmnpqrstuvwxyz"[rem];
            }
            return new string(uidChars);
        }

        /// <summary>
        /// Given a data string to be inserted, insert it
        /// and return the unique key generated for it.
        /// </summary>
        /// <param name="data">The data to be inserted</param>
        /// <returns>The key to the data</returns>

        public string Insert(string data)
            => Insert(UniqueId(), data);

        /// <summary>
        /// Given a data string to be inserted, insert it
        /// and return the key associated with it.
        /// </summary>
        /// <param name="key">The key to be associated with the data</param>
        /// <param name="data">The data to be inserted</param>
        /// <returns>The key to the data</returns>

        public string Insert(string key, string data)
        {
            cache[key] = data;
            return key;
        }

        /// <summary>
        /// Return true if the cache contains the specified key
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <returns>True if the key is in the dictionary</returns>

        public bool Contains(string key)
            => cache.ContainsKey(key);

        /// <summary>
        /// Retrieve data associated with a key from the cache
        /// </summary>
        /// <param name="key">The key whose data we need</param>
        /// <returns>The data for that key</returns>

        public string Get(string key)
            => cache[key];
    }
}
