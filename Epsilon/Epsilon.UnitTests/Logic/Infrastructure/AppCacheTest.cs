using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Infrastructure;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Wrappers;
using Epsilon.Logic.Wrappers.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Infrastructure
{
    [TestFixture]
    public class AppCacheTest
    {
        private class TestCacheable
        {
            public string Name { get; set; }
            public double SlidingWindowExpirationInSeconds { get; set; }
        }

        private ICacheWrapper _cacheWrapper = new HttpRuntimeCache();

        #region Setup

        [SetUp]
        public void TestSetup()
        {
            _cacheWrapper.Clear();
        }

        #endregion

        #region AllKeys

        [Test]
        public void AllKeys_Test()
        {
            var key1 = "key1";
            var key2 = "key2";
            var keys = new List<string> { key1, key2 };

            var cacheWrapper = CreateCacheWrapperForAllKeys(keys);
            var config = CreateConfig();
            var appCache = new AppCache(cacheWrapper, config);

            var keysReturned = appCache.AllKeys().ToList();

            Assert.AreEqual(keys.Count, keysReturned.Count,
                "The number of keys is not the expected.");
            Assert.IsTrue(keysReturned.Contains(key1), "Key1 was not found.");
            Assert.IsTrue(keysReturned.Contains(key2), "Key2 was not found.");
        }

        #endregion

        #region Clear

        [Test]
        public void Clear_Test()
        {
            bool clearCalled = false;

            var cacheWrapper = CreateCacheWrapperForClear(() => clearCalled = true);
            var config = CreateConfig();
            var appCache = new AppCache(cacheWrapper, config);

            appCache.Clear();

            Assert.IsTrue(clearCalled, "Clear was not called on the underlying CacheWrapper.");
        }

        #endregion

        #region ContainsKey

        [Test]
        public void ContainsKey_Test()
        {
            var cacheWrapper = CreateCacheWrapperForContainsKey(key => key.StartsWith("yes"));
            var config = CreateConfig();
            var appCache = new AppCache(cacheWrapper, config);

            appCache.Clear();

            Assert.IsTrue(appCache.ContainsKey("yes-1"), "Case 1 failed.");
            Assert.IsTrue(appCache.ContainsKey("yes-2"), "Case 2 failed.");
            Assert.IsFalse(appCache.ContainsKey("no-3"), "Case 3 failed.");
            Assert.IsFalse(appCache.ContainsKey("no-4"), "Case 4 failed.");
        }

        #endregion

        #region Get
        
        [Test]
        public void Get_CanHandleNulls_WithLock()
        {
            var withLock = WithLock.Yes;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";

            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = appCache.Get<string>(key, () => null, withLock);
            Assert.IsTrue(_cacheWrapper.ContainsKey(key), "Key should be in cache after.");
            var value2 = appCache.Get(key, () => "value-not-to-be-used", withLock);

            Assert.IsNull(value1, "Value1 should be null.");
            Assert.IsNull(value2, "Value2 should be null.");
        }

        [Test]
        public void Get_CanHandleNulls_WithoutLock()
        {
            var withLock = WithLock.No;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";

            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = appCache.Get<string>(key, () => null, withLock);
            Assert.IsTrue(_cacheWrapper.ContainsKey(key), "Key should be in cache after.");
            var value2 = appCache.Get(key, () => "value-not-to-be-used", withLock);

            Assert.IsNull(value1, "Value1 should be null.");
            Assert.IsNull(value2, "Value2 should be null.");
        }

        [Test]
        public void Get_GetItemCallbackIsNotUsedTheSecondTime_WithLock()
        {
            var withLock = WithLock.Yes;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var value = "value";

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = appCache.Get(key, () => { callbackCalled = true; return value; }, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");

            callbackCalled = false;
            Assert.IsTrue(_cacheWrapper.ContainsKey(key), "Key should be in cache after the first time.");
            var value2 = appCache.Get(key, () => { callbackCalled = true; return "value-not-to-be-used"; }, withLock);
            Assert.IsFalse(callbackCalled, "GetItemCallback should not be called the second time.");

            Assert.AreEqual(value, value1, "Value1 is not the expected.");
            Assert.AreEqual(value, value2, "Value2 is not the expected.");
        }

        [Test]
        public void Get_GetItemCallbackIsNotUsedTheSecondTime_WithoutLock()
        {
            var withLock = WithLock.No;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var value = "value";

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = appCache.Get(key, () => { callbackCalled = true; return value; }, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");

            callbackCalled = false;
            Assert.IsTrue(_cacheWrapper.ContainsKey(key), "Key should be in cache after the first time.");
            var value2 = appCache.Get(key, () => { callbackCalled = true; return "value-not-to-be-used"; }, withLock);
            Assert.IsFalse(callbackCalled, "GetItemCallback should not be called the second time.");

            Assert.AreEqual(value, value1, "Value1 is not the expected.");
            Assert.AreEqual(value, value2, "Value2 is not the expected.");
        }

        [Test]
        public void Get_CachingDisabled_WithLock()
        {
            var withLock = WithLock.Yes;
            var config = CreateConfig(disableAppCache: true);
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var value = "value";

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = appCache.Get(key, () => { callbackCalled = true; return value; }, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");
            Assert.AreEqual(value, value1, "Value1 is not the expected.");

            var newValue = "new-value";
            callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache after the first time.");
            var value2 = appCache.Get(key, () => { callbackCalled = true; return newValue; }, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback should be called the second time.");
            Assert.AreEqual(newValue, value2, "Value2 is not the expected.");
        }

        [Test]
        public void Get_CachingDisabled_WithoutLock()
        {
            var withLock = WithLock.No;
            var config = CreateConfig(disableAppCache: true);
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var value = "value";

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = appCache.Get(key, () => { callbackCalled = true; return value; }, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");
            Assert.AreEqual(value, value1, "Value1 is not the expected.");

            var newValue = "new-value";
            callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache after the first time.");
            var value2 = appCache.Get(key, () => { callbackCalled = true; return newValue; }, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback should be called the second time.");
            Assert.AreEqual(newValue, value2, "Value2 is not the expected.");
        }

        [Test]
        public void Get_WithSlidingExpiration_WithoutLock()
        {
            var withLock = WithLock.No;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var value = "value";
            var slidingExpirationInSeconds = 0.1;
            var slidingExpiration = TimeSpan.FromSeconds(slidingExpirationInSeconds);

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = appCache.Get(key, () => { callbackCalled = true; return value; }, slidingExpiration, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");
            Assert.AreEqual(value, value1, "Value1 is not the expected.");
            Assert.IsTrue(_cacheWrapper.ContainsKey(key), 
                "Key is not present in the underlying cache wrapper before the expiration.");

            var newValue = "new-value";
            callbackCalled = false;
            var value2 = appCache.Get(key, () => { callbackCalled = true; return newValue; }, withLock);
            Assert.IsFalse(callbackCalled, "GetItemCallback should not be called the second time.");
            Assert.AreEqual(value, value2, "Value2 is not the expected.");

            Thread.Sleep(slidingExpiration);
            Assert.IsFalse(_cacheWrapper.ContainsKey(key),
                "Key should not be present in the underlying cache wrapper after the expiration.");
        }

        [Test]
        public void Get_WithSlidingExpiration_WithLock()
        {
            var withLock = WithLock.Yes;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var value = "value";
            var slidingExpirationInSeconds = 0.1;
            var slidingExpiration = TimeSpan.FromSeconds(slidingExpirationInSeconds);

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = appCache.Get(key, () => { callbackCalled = true; return value; }, slidingExpiration, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");
            Assert.AreEqual(value, value1, "Value1 is not the expected.");
            Assert.IsTrue(_cacheWrapper.ContainsKey(key),
                "Key is not present in the underlying cache wrapper before the expiration.");

            var newValue = "new-value";
            callbackCalled = false;
            var value2 = appCache.Get(key, () => { callbackCalled = true; return newValue; }, withLock);
            Assert.IsFalse(callbackCalled, "GetItemCallback should not be called the second time.");
            Assert.AreEqual(value, value2, "Value2 is not the expected.");

            Thread.Sleep(slidingExpiration);
            Assert.IsFalse(_cacheWrapper.ContainsKey(key),
                "Key should not be present in the underlying cache wrapper after the expiration.");
        }

        [Test]
        public void Get_WithSlidingExpirationFunc_WithoutLock()
        {
            var withLock = WithLock.No;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var slidingExpirationInSeconds = 0.1;
            var defaultSlidingExpirationInSeconds = 60.0;
            var value = new TestCacheable
            {
                Name = "name",
                SlidingWindowExpirationInSeconds = slidingExpirationInSeconds
            };
            var defaultSlidingExpiration = TimeSpan.FromSeconds(defaultSlidingExpirationInSeconds);

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = appCache.Get(key, 
                () => { callbackCalled = true; return value; }, 
                x => TimeSpan.FromSeconds(x.SlidingWindowExpirationInSeconds) , defaultSlidingExpiration, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");
            Assert.AreEqual(value.Name, value1.Name, "Value1.Name is not the expected.");
            Assert.IsTrue(_cacheWrapper.ContainsKey(key),
                "Key is not present in the underlying cache wrapper before the expiration.");

            var newValue = new TestCacheable
            {
                Name = "new-name",
                SlidingWindowExpirationInSeconds = slidingExpirationInSeconds * 100
            };
            callbackCalled = false;
            var value2 = appCache.Get(key, 
                () => { callbackCalled = true; return newValue; },
                x => TimeSpan.FromSeconds(x.SlidingWindowExpirationInSeconds), 
                defaultSlidingExpiration, withLock);
            Assert.IsFalse(callbackCalled, "GetItemCallback should not be called the second time.");
            Assert.AreEqual(value.Name, value2.Name, "Value2.Name is not the expected.");

            Thread.Sleep(TimeSpan.FromSeconds(value.SlidingWindowExpirationInSeconds));
            Assert.IsFalse(_cacheWrapper.ContainsKey(key),
                "Key should not be present in the underlying cache wrapper after the expiration.");
        }

        [Test]
        public void Get_WithSlidingExpirationFunc_WithLock()
        {
            var withLock = WithLock.Yes;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var slidingExpirationInSeconds = 0.1;
            var defaultSlidingExpirationInSeconds = 60.0;
            var value = new TestCacheable
            {
                Name = "name",
                SlidingWindowExpirationInSeconds = slidingExpirationInSeconds
            };
            var defaultSlidingExpiration = TimeSpan.FromSeconds(defaultSlidingExpirationInSeconds);

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = appCache.Get(key,
                () => { callbackCalled = true; return value; },
                x => TimeSpan.FromSeconds(x.SlidingWindowExpirationInSeconds), defaultSlidingExpiration, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");
            Assert.AreEqual(value.Name, value1.Name, "Value1.Name is not the expected.");
            Assert.IsTrue(_cacheWrapper.ContainsKey(key),
                "Key is not present in the underlying cache wrapper before the expiration.");

            var newValue = new TestCacheable
            {
                Name = "new-name",
                SlidingWindowExpirationInSeconds = slidingExpirationInSeconds * 100
            };
            callbackCalled = false;
            var value2 = appCache.Get(key,
                () => { callbackCalled = true; return newValue; },
                x => TimeSpan.FromSeconds(x.SlidingWindowExpirationInSeconds),
                defaultSlidingExpiration, withLock);
            Assert.IsFalse(callbackCalled, "GetItemCallback should not be called the second time.");
            Assert.AreEqual(value.Name, value2.Name, "Value2.Name is not the expected.");

            Thread.Sleep(TimeSpan.FromSeconds(value.SlidingWindowExpirationInSeconds));
            Assert.IsFalse(_cacheWrapper.ContainsKey(key),
                "Key should not be present in the underlying cache wrapper after the expiration.");
        }

        [Test]
        public void Get_WithSlidingExpirationFunc_UsesDefaultWhenCallbackReturnsNull_WithoutLock()
        {
            var withLock = WithLock.No;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var slidingExpirationInSeconds = 60.0;
            var defaultSlidingExpirationInSeconds = 0.1;
            var defaultSlidingExpiration = TimeSpan.FromSeconds(defaultSlidingExpirationInSeconds);

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = appCache.Get<TestCacheable>(key,
                () => { callbackCalled = true; return null; },
                x => TimeSpan.FromSeconds(x.SlidingWindowExpirationInSeconds), defaultSlidingExpiration, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");
            Assert.IsNull(value1, "Value1 should be null.");
            Assert.IsTrue(_cacheWrapper.ContainsKey(key),
                "Key is not present in the underlying cache wrapper before the expiration.");

            var newValue = new TestCacheable
            {
                Name = "new-name",
                SlidingWindowExpirationInSeconds = slidingExpirationInSeconds * 100
            };
            callbackCalled = false;
            var value2 = appCache.Get(key,
                () => { callbackCalled = true; return newValue; },
                x => TimeSpan.FromSeconds(x.SlidingWindowExpirationInSeconds),
                defaultSlidingExpiration, withLock);
            Assert.IsFalse(callbackCalled, "GetItemCallback should not be called the second time.");
            Assert.IsNull(value2, "Value2 shold be null.");

            Thread.Sleep(TimeSpan.FromSeconds(defaultSlidingExpirationInSeconds));
            Assert.IsFalse(_cacheWrapper.ContainsKey(key),
                "Key should not be present in the underlying cache wrapper after the expiration.");
        }

        [Test]
        public void Get_WithSlidingExpirationFunc_UsesDefaultWhenCallbackReturnsNull_WithLock()
        {
            var withLock = WithLock.Yes;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var slidingExpirationInSeconds = 60.0;
            var defaultSlidingExpirationInSeconds = 0.1;
            var defaultSlidingExpiration = TimeSpan.FromSeconds(defaultSlidingExpirationInSeconds);

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = appCache.Get<TestCacheable>(key,
                () => { callbackCalled = true; return null; },
                x => TimeSpan.FromSeconds(x.SlidingWindowExpirationInSeconds), defaultSlidingExpiration, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");
            Assert.IsNull(value1, "Value1 should be null.");
            Assert.IsTrue(_cacheWrapper.ContainsKey(key),
                "Key is not present in the underlying cache wrapper before the expiration.");

            var newValue = new TestCacheable
            {
                Name = "new-name",
                SlidingWindowExpirationInSeconds = slidingExpirationInSeconds * 100
            };
            callbackCalled = false;
            var value2 = appCache.Get(key,
                () => { callbackCalled = true; return newValue; },
                x => TimeSpan.FromSeconds(x.SlidingWindowExpirationInSeconds),
                defaultSlidingExpiration, withLock);
            Assert.IsFalse(callbackCalled, "GetItemCallback should not be called the second time.");
            Assert.IsNull(value2, "Value2 shold be null.");

            Thread.Sleep(TimeSpan.FromSeconds(defaultSlidingExpirationInSeconds));
            Assert.IsFalse(_cacheWrapper.ContainsKey(key),
                "Key should not be present in the underlying cache wrapper after the expiration.");
        }

        #endregion GetAsync

        [Test]
        public async Task GetAsync_CanHandleNulls_WithLock()
        {
            var withLock = WithLock.Yes;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";

            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = await appCache.GetAsync(key, () => Task.FromResult<string>(null), withLock);
            Assert.IsTrue(_cacheWrapper.ContainsKey(key), "Key should be in cache after.");
            var value2 = await appCache.GetAsync(key, () => Task.FromResult<string>(null), withLock);

            Assert.IsNull(value1, "Value1 should be null.");
            Assert.IsNull(value2, "Value2 should be null.");
        }

        [Test]
        public async Task GetAsync_CanHandleNulls_WithoutLock()
        {
            var withLock = WithLock.No;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";

            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = await appCache.GetAsync(key, () => Task.FromResult<string>(null), withLock);
            Assert.IsTrue(_cacheWrapper.ContainsKey(key), "Key should be in cache after.");
            var value2 = await appCache.GetAsync(key, () => Task.FromResult<string>(null), withLock);

            Assert.IsNull(value1, "Value1 should be null.");
            Assert.IsNull(value2, "Value2 should be null.");
        }

        [Test]
        public async Task GetAsync_GetItemCallbackIsNotUsedTheSecondTime_WithLock()
        {
            var withLock = WithLock.Yes;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var value = "value";

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = await appCache.GetAsync(key, 
                () => { callbackCalled = true; return Task.FromResult(value); }, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");

            callbackCalled = false;
            Assert.IsTrue(_cacheWrapper.ContainsKey(key), "Key should be in cache after the first time.");
            var value2 = await appCache.GetAsync(key, 
                () => { callbackCalled = true; return Task.FromResult("value-not-to-be-used"); }, withLock);
            Assert.IsFalse(callbackCalled, "GetItemCallback should not be called the second time.");

            Assert.AreEqual(value, value1, "Value1 is not the expected.");
            Assert.AreEqual(value, value2, "Value2 is not the expected.");
        }

        [Test]
        public async Task GetAsync_GetItemCallbackIsNotUsedTheSecondTime_WithoutLock()
        {
            var withLock = WithLock.No;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var value = "value";

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = await appCache.GetAsync(key, 
                () => { callbackCalled = true; return Task.FromResult(value); }, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");

            callbackCalled = false;
            Assert.IsTrue(_cacheWrapper.ContainsKey(key), "Key should be in cache after the first time.");
            var value2 = await appCache.GetAsync(key, 
                () => { callbackCalled = true; return Task.FromResult("value-not-to-be-used"); }, withLock);
            Assert.IsFalse(callbackCalled, "GetItemCallback should not be called the second time.");

            Assert.AreEqual(value, value1, "Value1 is not the expected.");
            Assert.AreEqual(value, value2, "Value2 is not the expected.");
        }

        [Test]
        public async Task GetAsync_CachingDisabled_WithLock()
        {
            var withLock = WithLock.Yes;
            var config = CreateConfig(disableAppCache: true);
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var value = "value";

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = await appCache.GetAsync(key,
                () => { callbackCalled = true; return Task.FromResult(value); }, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");
            Assert.AreEqual(value, value1, "Value1 is not the expected.");

            var newValue = "new-value";
            callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache after the first time.");
            var value2 = await appCache.GetAsync(key,
                () => { callbackCalled = true; return Task.FromResult(newValue); }, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback should be called the second time.");
            Assert.AreEqual(newValue, value2, "Value2 is not the expected.");
        }

        [Test]
        public async Task GetAsync_CachingDisabled_WithoutLock()
        {
            var withLock = WithLock.No;
            var config = CreateConfig(disableAppCache: true);
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var value = "value";

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = await appCache.GetAsync(key,
                () => { callbackCalled = true; return Task.FromResult(value); }, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");
            Assert.AreEqual(value, value1, "Value1 is not the expected.");

            var newValue = "new-value";
            callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache after the first time.");
            var value2 = await appCache.GetAsync(key,
                () => { callbackCalled = true; return Task.FromResult(newValue); }, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback should be called the second time.");
            Assert.AreEqual(newValue, value2, "Value2 is not the expected.");
        }

        [Test]
        public async Task GetAsync_WithSlidingExpiration_WithoutLock()
        {
            var withLock = WithLock.No;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var value = "value";
            var slidingExpirationInSeconds = 0.1;
            var slidingExpiration = TimeSpan.FromSeconds(slidingExpirationInSeconds);

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = await appCache.GetAsync(key, 
                () => { callbackCalled = true; return Task.FromResult(value); }, slidingExpiration, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");
            Assert.AreEqual(value, value1, "Value1 is not the expected.");
            Assert.IsTrue(_cacheWrapper.ContainsKey(key),
                "Key is not present in the underlying cache wrapper before the expiration.");

            var newValue = "new-value";
            callbackCalled = false;
            var value2 = await appCache.GetAsync(key, 
                () => { callbackCalled = true; return Task.FromResult(newValue); }, withLock);
            Assert.IsFalse(callbackCalled, "GetItemCallback should not be called the second time.");
            Assert.AreEqual(value, value2, "Value2 is not the expected.");

            await Task.Delay(slidingExpiration);

            Assert.IsFalse(_cacheWrapper.ContainsKey(key),
                "Key should not be present in the underlying cache wrapper after the expiration.");
        }

        [Test]
        public async Task GetAsync_WithSlidingExpiration_WithLock()
        {
            var withLock = WithLock.Yes;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var value = "value";
            var slidingExpirationInSeconds = 0.1;
            var slidingExpiration = TimeSpan.FromSeconds(slidingExpirationInSeconds);

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = await appCache.GetAsync(key,
                () => { callbackCalled = true; return Task.FromResult(value); }, slidingExpiration, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");
            Assert.AreEqual(value, value1, "Value1 is not the expected.");
            Assert.IsTrue(_cacheWrapper.ContainsKey(key),
                "Key is not present in the underlying cache wrapper before the expiration.");

            var newValue = "new-value";
            callbackCalled = false;
            var value2 = await appCache.GetAsync(key,
                () => { callbackCalled = true; return Task.FromResult(newValue); }, withLock);
            Assert.IsFalse(callbackCalled, "GetItemCallback should not be called the second time.");
            Assert.AreEqual(value, value2, "Value2 is not the expected.");

            await Task.Delay(slidingExpiration);

            Assert.IsFalse(_cacheWrapper.ContainsKey(key),
                "Key should not be present in the underlying cache wrapper after the expiration.");
        }

        [Test]
        public async Task GetAsync_WithSlidingExpirationFunc_WithoutLock()
        {
            var withLock = WithLock.No;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var slidingExpirationInSeconds = 0.1;
            var defaultSlidingExpirationInSeconds = 60.0;
            var value = new TestCacheable
            {
                Name = "name",
                SlidingWindowExpirationInSeconds = slidingExpirationInSeconds
            };
            var defaultSlidingExpiration = TimeSpan.FromSeconds(defaultSlidingExpirationInSeconds);

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = await appCache.GetAsync(key,
                () => { callbackCalled = true; return Task.FromResult(value); },
                x => TimeSpan.FromSeconds(x.SlidingWindowExpirationInSeconds), defaultSlidingExpiration, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");
            Assert.AreEqual(value.Name, value1.Name, "Value1.Name is not the expected.");
            Assert.IsTrue(_cacheWrapper.ContainsKey(key),
                "Key is not present in the underlying cache wrapper before the expiration.");

            var newValue = new TestCacheable
            {
                Name = "new-name",
                SlidingWindowExpirationInSeconds = slidingExpirationInSeconds * 100
            };
            callbackCalled = false;
            var value2 = await appCache.GetAsync(key,
                () => { callbackCalled = true; return Task.FromResult(newValue); },
                x => TimeSpan.FromSeconds(x.SlidingWindowExpirationInSeconds),
                defaultSlidingExpiration, withLock);
            Assert.IsFalse(callbackCalled, "GetItemCallback should not be called the second time.");
            Assert.AreEqual(value.Name, value2.Name, "Value2.Name is not the expected.");

            Thread.Sleep(TimeSpan.FromSeconds(value.SlidingWindowExpirationInSeconds));
            Assert.IsFalse(_cacheWrapper.ContainsKey(key),
                "Key should not be present in the underlying cache wrapper after the expiration.");
        }

        [Test]
        public async Task GetAsync_WithSlidingExpirationFunc_UsesDefaultWhenCallbackReturnsNull_WithoutLock()
        {
            var withLock = WithLock.No;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var slidingExpirationInSeconds = 60.0;
            var defaultSlidingExpirationInSeconds = 0.1;
            var defaultSlidingExpiration = TimeSpan.FromSeconds(defaultSlidingExpirationInSeconds);

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = await appCache.GetAsync(key,
                () => { callbackCalled = true; return Task.FromResult<TestCacheable>(null); },
                x => TimeSpan.FromSeconds(x.SlidingWindowExpirationInSeconds), defaultSlidingExpiration, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");
            Assert.IsNull(value1, "Value1 should be null.");
            Assert.IsTrue(_cacheWrapper.ContainsKey(key),
                "Key is not present in the underlying cache wrapper before the expiration.");

            var newValue = new TestCacheable
            {
                Name = "new-name",
                SlidingWindowExpirationInSeconds = slidingExpirationInSeconds * 100
            };
            callbackCalled = false;
            var value2 = await appCache.GetAsync(key,
                () => { callbackCalled = true; return Task.FromResult(newValue); },
                x => TimeSpan.FromSeconds(x.SlidingWindowExpirationInSeconds),
                defaultSlidingExpiration, withLock);
            Assert.IsFalse(callbackCalled, "GetItemCallback should not be called the second time.");
            Assert.IsNull(value2, "Value2 shold be null.");

            Thread.Sleep(TimeSpan.FromSeconds(defaultSlidingExpirationInSeconds));
            Assert.IsFalse(_cacheWrapper.ContainsKey(key),
                "Key should not be present in the underlying cache wrapper after the expiration.");
        }

        [Test]
        public async Task GetAsync_WithSlidingExpirationFunc_UsesDefaultWhenCallbackReturnsNull_WithLock()
        {
            var withLock = WithLock.Yes;
            var config = CreateConfig();
            var appCache = new AppCache(_cacheWrapper, config);

            var key = "key";
            var slidingExpirationInSeconds = 60.0;
            var defaultSlidingExpirationInSeconds = 0.1;
            var defaultSlidingExpiration = TimeSpan.FromSeconds(defaultSlidingExpirationInSeconds);

            var callbackCalled = false;
            Assert.IsFalse(_cacheWrapper.ContainsKey(key), "Key should not be in cache before.");
            var value1 = await appCache.GetAsync(key,
                () => { callbackCalled = true; return Task.FromResult<TestCacheable>(null); },
                x => TimeSpan.FromSeconds(x.SlidingWindowExpirationInSeconds), defaultSlidingExpiration, withLock);
            Assert.IsTrue(callbackCalled, "GetItemCallback was not called the first time.");
            Assert.IsNull(value1, "Value1 should be null.");
            Assert.IsTrue(_cacheWrapper.ContainsKey(key),
                "Key is not present in the underlying cache wrapper before the expiration.");

            var newValue = new TestCacheable
            {
                Name = "new-name",
                SlidingWindowExpirationInSeconds = slidingExpirationInSeconds * 100
            };
            callbackCalled = false;
            var value2 = await appCache.GetAsync(key,
                () => { callbackCalled = true; return Task.FromResult(newValue); },
                x => TimeSpan.FromSeconds(x.SlidingWindowExpirationInSeconds),
                defaultSlidingExpiration, withLock);
            Assert.IsFalse(callbackCalled, "GetItemCallback should not be called the second time.");
            Assert.IsNull(value2, "Value2 shold be null.");

            Thread.Sleep(TimeSpan.FromSeconds(defaultSlidingExpirationInSeconds));
            Assert.IsFalse(_cacheWrapper.ContainsKey(key),
                "Key should not be present in the underlying cache wrapper after the expiration.");
        }

        #region


        #endregion

        #region Remove

        [Test]
        public void Remove_Test()
        {
            var keyToRemove = "key";
            string keyUsed = null;

            var cacheWrapper = CreateCacheWrapperForRemove(key => keyUsed = key);
            var config = CreateConfig();
            var appCache = new AppCache(cacheWrapper, config);

            appCache.Remove(keyToRemove);

            Assert.AreEqual(keyToRemove, keyUsed,
                "The key used when calling Remove on the underlying cache wrapper is not the expected.");
        }

        #endregion

        #region Private Helpers

        private ICacheWrapper CreateCacheWrapperForAllKeys(IEnumerable<string> keys)
        {
            var mockCacheWrapper = new Mock<ICacheWrapper>();

            mockCacheWrapper.Setup(x => x.AllKeys()).Returns(keys);

            return mockCacheWrapper.Object;
        }

        private ICacheWrapper CreateCacheWrapperForClear(Action clearCallback)
        {
            var mockCacheWrapper = new Mock<ICacheWrapper>();

            mockCacheWrapper.Setup(x => x.Clear()).Callback(clearCallback);

            return mockCacheWrapper.Object;
        }

        private ICacheWrapper CreateCacheWrapperForContainsKey(Func<string, bool> containsValueFunc)
        {
            var mockCacheWrapper = new Mock<ICacheWrapper>();

            mockCacheWrapper.Setup(x => x.ContainsKey(It.IsAny<string>()))
                .Returns(containsValueFunc);

            return mockCacheWrapper.Object;
        }

        private ICacheWrapper CreateCacheWrapperForRemove(Action<string> removeCallback)
        {
            var mockCacheWrapper = new Mock<ICacheWrapper>();

            mockCacheWrapper.Setup(x => x.Remove(It.IsAny<string>())).Callback(removeCallback);

            return mockCacheWrapper.Object;
        }

        private IAppCacheConfig CreateConfig(
            bool disableAppCache = false, 
            bool disableAsynchronousLocking = false, 
            bool disableSynchronousLocking = false)
        {
            var mockConfig = new Mock<IAppCacheConfig>();

            mockConfig.Setup(x => x.DisableAppCache).Returns(disableAppCache);
            mockConfig.Setup(x => x.DisableAsynchronousLocking).Returns(disableAsynchronousLocking);
            mockConfig.Setup(x => x.DisableSynchronousLocking).Returns(disableSynchronousLocking);

            return mockConfig.Object;
        }

        #endregion
    }
}
