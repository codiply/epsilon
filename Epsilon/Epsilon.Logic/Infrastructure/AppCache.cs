using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Infrastructure
{
    public class AppCache : IAppCache
    {
        private static ConcurrentDictionary<string, object> _locks = 
            new ConcurrentDictionary<string, object>(); 
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

        public T Get<T>(
            string key, Func<T> getItemCallback, WithLock lockOption) where T : class
        {
            return GenericGet(key, getItemCallback, (c, k, o) => c.Insert(k, o), lockOption);
        }

        public T Get<T>(
            string key, Func<T> getItemCallback, TimeSpan slidingExpiration, WithLock lockOption) where T : class
        {
            return GenericGet(key, getItemCallback, (c, k, o) => c.Insert(k, o, slidingExpiration), lockOption);
        }

        public T Get<T>(
            string key, Func<T> getItemCallback, DateTime absoluteExpiration, WithLock lockOption) where T : class
        {
            return GenericGet(key, getItemCallback, (c, k, o) => c.Insert(k, o, absoluteExpiration), lockOption);
        }

        public async Task<T> GetAsync<T>(
            string key, Func<Task<T>> getItemCallback, WithLock lockOption) where T : class
        {
            return await GenericGetAsync(key, getItemCallback, (c, k, o) => c.Insert(k, o), lockOption);
        }

        public async Task<T> GetAsync<T>(
            string key, Func<Task<T>> getItemCallback, TimeSpan slidingExpiration, WithLock lockOption) where T : class
        {
            return await GenericGetAsync(key, getItemCallback, (c, k, o) => c.Insert(k, o, slidingExpiration), lockOption);
        }

        public async Task<T> GetAsync<T>(
            string key, Func<Task<T>> getItemCallback, DateTime absoluteExpiration, WithLock lockOption) where T : class
        {
            return await GenericGetAsync(key, getItemCallback, (c, k, o) => c.Insert(k, o, absoluteExpiration), lockOption);
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

        private static object GetLock(string key)
        {
            return _locks.GetOrAdd(key, x => new Object());
        }

        private T GenericGet<T>(
            string key, Func<T> getItemCallback, Action<ICacheWrapper, string, Object> insertFunc, WithLock lockOption) where T : class
        {
            switch (lockOption)
            {
                case WithLock.Yes:
                    return GenericGetWithLock<T>(key, getItemCallback, insertFunc);
                case WithLock.No:
                    return GenericGetWithoutLock<T>(key, getItemCallback, insertFunc);
                default:
                    throw new NotImplementedException();
            }
        }

        private T GenericGetWithLock<T>(string key, Func<T> getItemCallback, Action<ICacheWrapper, string, Object> insertFunc) where T : class
        {
            bool shouldReturnNull;
            T item = GetFromCache<T>(key, out shouldReturnNull);
            if (shouldReturnNull)
                return null;

            if (item != null)
                return item;

            lock (GetLock(key))
            {
                // Check now that you aquired the lock that the item is still absent.
                item = _cache.Get(key) as T;

                if (item != null)
                    return item;

                // If the item was not found in the cache,
                // get it using the callback function. 
                item = getItemCallback();

                Insert(key, item, insertFunc);
            }

            return item;
        }

        private T GenericGetWithoutLock<T>(string key, Func<T> getItemCallback, Action<ICacheWrapper, string, Object> insertFunc) where T : class
        {
            bool shouldReturnNull;
            T item = GetFromCache<T>(key, out shouldReturnNull);
            if (shouldReturnNull)
                return null;

            if (item != null)
                return item;

            // If the item was not found in the cache,
            // get it using the callback function. 
            item = getItemCallback();

            Insert(key, item, insertFunc);

            return item;
        }

        private async Task<T> GenericGetAsync<T>(
            string key, Func<Task<T>> getItemCallback, Action<ICacheWrapper, string, Object> insertFunc, WithLock lockOption) where T : class
        {
            switch (lockOption)
            {
                case WithLock.Yes:
                    return await GenericGetWithLockAsync<T>(key, getItemCallback, insertFunc);
                case WithLock.No:
                    return await GenericGetWithoutLockAsync<T>(key, getItemCallback, insertFunc);
                default:
                    throw new NotImplementedException();
            }
        }

        private async Task<T> GenericGetWithLockAsync<T>(string key, Func<Task<T>> getItemCallback, Action<ICacheWrapper, string, Object> insertFunc) where T : class
        {
            // TODO: Make this method use an asynchronous locking mechanism.
            throw new NotImplementedException();

            bool shouldReturnNull;
            T item = GetFromCache<T>(key, out shouldReturnNull);
            if (shouldReturnNull)
                return null;

            if (item == null)
            {
                // If the item was not found in the cache,
                // get it using the callback function. 
                item = await getItemCallback();

                Insert(key, item, insertFunc);
            }

            return item;
        }

        private async Task<T> GenericGetWithoutLockAsync<T>(string key, Func<Task<T>> getItemCallback, Action<ICacheWrapper, string, Object> insertFunc) where T : class
        {
            bool shouldReturnNull;
            T item = GetFromCache<T>(key, out shouldReturnNull);
            if (shouldReturnNull)
                return null;

            if (item == null)
            {
                // If the item was not found in the cache,
                // get it using the callback function. 
                item = await getItemCallback();

                Insert(key, item, insertFunc);
            }

            return item;
        }

        private void Insert<T>(string key, T item, Action<ICacheWrapper, string, Object> insertFunc)
        {
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

        private T GetFromCache<T>(string key, out bool shouldReturnNull) where T : class
        {
            // Try to get the value from the cache. It will be null if it is not there.
            var value = _cache.Get(key);

            // If you found something in the cache but it was not type T, return null.
            // This will also take care of the case you found a CacheableNull, 
            // in which case you need to return null. 
            shouldReturnNull = (value != null && !(value is T));

            // If you reach this point, the value is a T, possibly null.
            T item = value as T;

            return item;
        }
    }
}
