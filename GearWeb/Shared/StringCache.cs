using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GearWeb.Shared
{
    public sealed class StringCache
    {
        private static readonly StringCache instance = new StringCache();

        public static StringCache Instance => instance;

        // Ensure class not marked beforefieldinit by C# compiler
        static StringCache() { }
        private StringCache() { }

        private readonly Dictionary<string, string> cache
            = new Dictionary<string, string>();
        private int id = 0;
        private static readonly string idChars = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        
        private string UniqueId()
        {
            int v = Interlocked.Increment(ref id);
            char[] uidChars = new char[7];
            for (int i = 0; i < uidChars.Length; i++)
            {
                v = Math.DivRem(v, 24, out int rem);
                uidChars[i] = idChars[rem];
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
        /// <param name="key">The ley to look up</param>
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
