using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Infrastructure.Primitives;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Epsilon.Logic.Infrastructure
{
    public class AppCache : IAppCache
    {
        private static ConcurrentDictionary<string, object> _locks = 
            new ConcurrentDictionary<string, object>();
        private static ConcurrentDictionary<string, AsyncLock> _asyncLocks =
            new ConcurrentDictionary<string, AsyncLock>();
        private ICacheWrapper _cache;
        private IAppSettingsHelper _appSettingsHelper;

        public AppCache(
            ICacheWrapper cache,
            IAppSettingsHelper appSettingsHelper)
        {
            _cache = cache;
            _appSettingsHelper = appSettingsHelper;
        }

        public bool ContainsKey(string key)
        {
            return _cache.ContainsKey(key);
        }

        public T Get<T>(
            string key, Func<T> getItemCallback, WithLock lockOption) where T : class
        {
            return GenericGet(key, getItemCallback, null, null, lockOption);
        }

        public T Get<T>(
            string key,
            Func<T> getItemCallback,
            TimeSpan slidingExpiration,
            WithLock lockOption) where T : class
        {
            return GenericGet(key, getItemCallback, ignored => slidingExpiration, slidingExpiration, lockOption);
        }

        public T Get<T>(
            string key, 
            Func<T> getItemCallback, 
            Func<T, TimeSpan> slidingExpirationFunc,
            TimeSpan defaultSlidingExpiration,
            WithLock lockOption) where T : class
        {
            return GenericGet(key, getItemCallback, slidingExpirationFunc, defaultSlidingExpiration, lockOption);
        }

        public async Task<T> GetAsync<T>(
            string key, Func<Task<T>> getItemCallback, WithLock lockOption) where T : class
        {
            return await GenericGetAsync(key, getItemCallback, null, null, lockOption);
        }

        public async Task<T> GetAsync<T>(
            string key,
            Func<Task<T>> getItemCallback,
            TimeSpan slidingExpiration,
            WithLock lockOption) where T : class
        {
            return await GenericGetAsync(key, getItemCallback, ignored => slidingExpiration, slidingExpiration, lockOption);
        }

        public async Task<T> GetAsync<T>(
            string key,
            Func<Task<T>> getItemCallback,
            Func<T, TimeSpan> slidingExpirationFunc,
            TimeSpan defaultSlidingExpiration,
            WithLock lockOption) where T : class
        {
            return await GenericGetAsync(key, getItemCallback, slidingExpirationFunc, defaultSlidingExpiration, lockOption);
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

        private static AsyncLock GetAsyncLock(string key)
        {
            return _asyncLocks.GetOrAdd(key, x => new AsyncLock());
        }

        private T GenericGet<T>(
            string key, 
            Func<T> getItemCallback, 
            Func<T, TimeSpan> slidingExpirationFunc,
            TimeSpan? defaultSlidingExpiration,
            WithLock lockOption) where T : class
        {
            var disableCache = _appSettingsHelper.GetBool(AppSettingsKey.DisableAppCache) == true;
            if (disableCache)
                return getItemCallback();

            var disableLocking = _appSettingsHelper.GetBool(AppSettingsKey.DisableSynchronousLockingInAppCache) == true;
            if (disableLocking)
                lockOption = WithLock.No;

            switch (lockOption)
            {
                case WithLock.Yes:
                    return GenericGetWithLock<T>(key, getItemCallback, slidingExpirationFunc, defaultSlidingExpiration);
                case WithLock.No:
                    return GenericGetWithoutLock<T>(key, getItemCallback, slidingExpirationFunc, defaultSlidingExpiration);
                default:
                    throw new NotImplementedException();
            }
        }

        private T GenericGetWithLock<T>(
            string key, 
            Func<T> getItemCallback, 
            Func<T, TimeSpan> slidingExpirationFunc,
            TimeSpan? defaultSlidingExpiration) where T : class
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
                item = GetFromCache<T>(key, out shouldReturnNull);
                if (shouldReturnNull)
                    return null;

                if (item != null)
                    return item;

                // If the item was not found in the cache,
                // get it using the callback function. 
                item = getItemCallback();

                Insert(key, item, slidingExpirationFunc, defaultSlidingExpiration);
            }

            return item;
        }

        private T GenericGetWithoutLock<T>(
            string key, Func<T> getItemCallback, 
            Func<T, TimeSpan> slidingExpirationFunc, 
            TimeSpan? defaultSlidingExpiration) where T : class
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

            Insert(key, item, slidingExpirationFunc, defaultSlidingExpiration);

            return item;
        }

        private async Task<T> GenericGetAsync<T>(
            string key,
            Func<Task<T>> getItemCallback,
            Func<T, TimeSpan> slidingExpirationFunc,
            TimeSpan? defaultSlidingExpiration,
            WithLock lockOption) where T : class
        {
            var disableCache = _appSettingsHelper.GetBool(AppSettingsKey.DisableAppCache) == true;
            if (disableCache)
                return await getItemCallback();

            var disableLocking = _appSettingsHelper.GetBool(AppSettingsKey.DisableAsynchronousLockingInAppCache) == true;
            if (disableLocking)
                lockOption = WithLock.No;

            switch (lockOption)
            {
                case WithLock.Yes:
                    return await GenericGetWithLockAsync<T>(key, getItemCallback, slidingExpirationFunc, defaultSlidingExpiration);
                case WithLock.No:
                    return await GenericGetWithoutLockAsync<T>(key, getItemCallback, slidingExpirationFunc, defaultSlidingExpiration);
                default:
                    throw new NotImplementedException();
            }
        }

        private async Task<T> GenericGetWithLockAsync<T>(
            string key, 
            Func<Task<T>> getItemCallback,
            Func<T, TimeSpan> slidingExpirationFunc,
            TimeSpan? defaultSlidingExpiration) where T : class
        {
            bool shouldReturnNull;
            T item = GetFromCache<T>(key, out shouldReturnNull);
            if (shouldReturnNull)
                return null;

            if (item != null)
                return item;

            using (var releaser = await GetAsyncLock(key).LockAsync())
            {
                // Check now that you aquired the lock that the item is still absent.
                item = GetFromCache<T>(key, out shouldReturnNull);
                if (shouldReturnNull)
                    return null;

                if (item != null)
                    return item;

                // If the item was not found in the cache,
                // get it using the callback function. 
                item = await getItemCallback();

                Insert(key, item, slidingExpirationFunc, defaultSlidingExpiration);
            }

            return item;
        }

        private async Task<T> GenericGetWithoutLockAsync<T>(
            string key, Func<Task<T>> getItemCallback,
            Func<T, TimeSpan> slidingExpirationFunc,
            TimeSpan? defaultSlidingExpiration) where T : class
        {
            bool shouldReturnNull;
            T item = GetFromCache<T>(key, out shouldReturnNull);
            if (shouldReturnNull)
                return null;

            if (item != null)
                return item;

            // If the item was not found in the cache,
            // get it using the callback function. 
            item = await getItemCallback();

            Insert(key, item, slidingExpirationFunc, defaultSlidingExpiration);
            
            return item;
        }

        private void Insert<T>(string key, T item, Func<T, TimeSpan> slidingExpirationFunc, TimeSpan? defaultSlidingExpiration)
        {
            if (item == null)
            {
                // If the item is actually null, store a CacheableNull object instead.
                if (defaultSlidingExpiration.HasValue)
                {
                    _cache.Insert(key, new CacheableNull(), defaultSlidingExpiration.Value);
                }
                else
                {
                    _cache.Insert(key, new CacheableNull());
                }
            }
            else
            {
                // If the item is not null, store it in the cache.
                if (slidingExpirationFunc == null)
                {
                    _cache.Insert(key, item);
                }
                else
                {
                    _cache.Insert(key, item, slidingExpirationFunc(item));
                }
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
