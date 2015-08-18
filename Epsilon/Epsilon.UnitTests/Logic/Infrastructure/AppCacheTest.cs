using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Infrastructure;
using Epsilon.Logic.Wrappers.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Infrastructure
{
    [TestFixture]
    public class AppCacheTest
    {
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
