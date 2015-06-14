using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Infrastructure
{
    public class AppCache : IAppCache
    {
        private ICacheWrapper _cache;

        public AppCache(
            ICacheWrapper cache)
        {
            _cache = cache;
        }

        public bool ContainsKey(string key)
        {
            return _cache.ContainsKey(key);
        }

        public T Get<T>(string key, Func<T> getItemCallback) where T : class
        {
            return GenericGet(key, getItemCallback, (c, k, o) => c.Insert(k, o));
        }

        public T Get<T>(string key, Func<T> getItemCallback, TimeSpan slidingExpiration) where T : class
        {
            return GenericGet(key, getItemCallback, (c, k, o) => c.Insert(k, o, slidingExpiration));
        }

        public T Get<T>(string key, Func<T> getItemCallback, DateTime absoluteExpiration) where T : class
        {
            return GenericGet(key, getItemCallback, (c, k, o) => c.Insert(k, o, absoluteExpiration));
        }

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> getItemCallback) where T : class
        {
            return await GenericGetAsync(key, getItemCallback, (c, k, o) => c.Insert(k, o));
        }

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> getItemCallback, TimeSpan slidingExpiration) where T : class
        {
            return await GenericGetAsync(key, getItemCallback, (c, k, o) => c.Insert(k, o, slidingExpiration));
        }

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> getItemCallback, DateTime absoluteExpiration) where T : class
        {
            return await GenericGetAsync(key, getItemCallback, (c, k, o) => c.Insert(k, o, absoluteExpiration));
        }

        private T GenericGet<T>(string key, Func<T> getItemCallback, Action<ICacheWrapper, string, Object> insertFunc) where T : class
        {
            // Try to get the value from the cache. It will be null if it is not there.
            var value = _cache.Get(key);

            // If you found something in the cache but it was not type T, return null.
            // This will also take care of the case you found a CacheableNull, 
            // in which case you need to return null. 
            if (value != null && !(value is T))
                return null;

            // If you reach this point, the value is a T, possibly null.
            T item = value as T;
            if (item != null)
                return item;

            var locker = "app-cache:lock-for-key:" + key;
            lock (locker)
            {
                item = _cache.Get(key) as T;

                if (item != null)
                    return item;

                // If the item was not found in the cache,
                // get it using the callback function. 
                item = getItemCallback();

                if (item == null)
                {
                    // If the item is actually null, store a CacheableNull object instead.
                    insertFunc(_cache, key, new CacheableNull());
                }
                else
                {
                    // If the item is not null, store it in the cache.
                    insertFunc(_cache, key, item);
                }
            }

            return item;
        }

        private async Task<T> GenericGetAsync<T>(string key, Func<Task<T>> getItemCallback, Action<ICacheWrapper, string, Object> insertFunc) where T : class
        {
            // Try to get the value from the cache. It will be null if it is not there.
            var value = _cache.Get(key);

            // If you found something in the cache but it was not type T, return null.
            // This will also take care of the case you found a CacheableNull, 
            // in which case you need to return null. 
            if (value != null && !(value is T))
                return null;

            // If you reach this point, the value is a T, possibly null.
            T item = value as T;

            if (item == null)
            {
                // If the item was not found in the cache,
                // get it using the callback function. 
                item = await getItemCallback();

                if (item == null)
                {
                    // If the item is actually null, store a CacheableNull object instead.
                    insertFunc(_cache, key, new CacheableNull());
                }
                else
                {
                    // If the item is not null, store it in the cache.
                    insertFunc(_cache, key, item);
                }
            }

            return item;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public IEnumerable<string> AllKeys()
        {
            return _cache.AllKeys();
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public class CacheableNull
        {
        }
    }
}
