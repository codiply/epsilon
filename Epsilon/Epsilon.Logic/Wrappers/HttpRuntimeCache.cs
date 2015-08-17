using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace Epsilon.Logic.Wrappers
{
    public class HttpRuntimeCache : ICacheWrapper
    {
        private Cache Cache
        {
            get { return HttpRuntime.Cache; }
        }

        public bool ContainsKey(string key)
        {
            return Cache.Get(key) != null;
        }

        public Object Get(string key)
        {
            return Cache.Get(key);
        }

        public void Insert(string key, Object value)
        {
            Cache.Insert(key, value);
        }

        public void Insert(string key, Object value, TimeSpan slidingExpiration)
        {
            Cache.Insert(key, value, null, Cache.NoAbsoluteExpiration, slidingExpiration);
        }

        public void Remove(string key)
        {
            Cache.Remove(key);
        }

        public IEnumerable<string> AllKeys()
        {
            var enumerator = Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return (string)enumerator.Key;
            }
        }

        public void Clear()
        {
            foreach (var key in AllKeys().ToList())
            {
                Cache.Remove(key);
            }
        }
    }
}
