using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace Epsilon.Logic.Wrappers
{
    public class HttpRuntimeCache : ICacheWrapper
    {
        private Cache _cache = HttpRuntime.Cache;

        public bool ContainsKey(string key)
        {
            return _cache.Get(key) != null;
        }

        public Object Get(string key)
        {
            return _cache.Get(key);
        }

        public void Insert(string key, Object value)
        {
            _cache.Insert(key, value);
        }

        public void Insert(string key, Object value, TimeSpan slidingExpiration)
        {
            _cache.Insert(key, value, null, Cache.NoAbsoluteExpiration, slidingExpiration);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public IEnumerable<string> AllKeys()
        {
            var enumerator = HttpRuntime.Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return (string)enumerator.Key;
            }
        }

        public void Clear()
        {
            foreach (var key in AllKeys().ToList())
            {
                _cache.Remove(key);
            }
        }
    }
}
