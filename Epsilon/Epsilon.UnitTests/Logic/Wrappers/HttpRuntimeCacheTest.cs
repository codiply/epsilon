using Epsilon.Logic.Wrappers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Wrappers
{
    [TestFixture]
    public class HttpRuntimeCacheTest
    {
        [Test]
        public void HttpRuntimeCacheCrashTest()
        {
            var longSlidingWindow = TimeSpan.FromMinutes(15.0);
            var shortSlidingWindow = TimeSpan.FromSeconds(0.01);
            var cache = new HttpRuntimeCache();

            int repeatNumberOfTimes = 10;

            for (var i = 0; i < repeatNumberOfTimes; i++)
            {
                cache.Clear();
                Assert.IsEmpty(cache.AllKeys(), "Cache should contain no keys to start with.");

                int numberOfStringsToCacheInititally = 10;

                // I cache lots of strings first.
                for (var j = 0; j < numberOfStringsToCacheInititally; j++)
                {
                    var key = "some_key_" + j;
                    var value = "some_value_" + j;
                    cache.Insert(key, value);
                    var retrievedValue = (string)cache.Get(key);
                    Assert.AreEqual(value, retrievedValue, string.Format("Check 1 failed for key '{0}'.", key));
                }

                var allKeys = cache.AllKeys();

                Assert.AreEqual(numberOfStringsToCacheInititally, allKeys.Count(),
                    "The number of keys in the cache is not the expected.");

                // I check they are still there
                for (var j = 0; j < numberOfStringsToCacheInititally; j++)
                {
                    var key = "some_key_" + j;
                    var value = "some_value_" + j;
                    var retrievedValue = (string)cache.Get(key);
                    Assert.AreEqual(value, retrievedValue, string.Format("Check 2 failed for key '{0}'.", key));
                    Assert.IsTrue(cache.ContainsKey(key),
                        string.Format("Contains method did not return the expected result for key '{0}'.", key));
                    Assert.IsTrue(allKeys.Contains(key),
                        string.Format("Key '{0}' was not found in AllKeys.", key));
                }

                // Retrieve value after expiration
                var key1 = "key_1";
                var value1 = "value_1";
                cache.Insert(key1, value1, shortSlidingWindow);
                Thread.Sleep(shortSlidingWindow);
                var retrievedValueForKey1 = (string)cache.Get(key1);
                Assert.IsNull(retrievedValueForKey1, "Retrieved value after expiration of sliding expiration should be null.");
                Assert.IsFalse(cache.ContainsKey(key1), "ContainsKey returned unexpected result for key 1.");

                // Retrieve value before and after removing it from cache.
                var key2 = "key_2";
                var value2 = "value_2";
                cache.Insert(key2, value2);
                var retrievedValueForKey2BeforeRemove = (string)cache.Get(key2);
                Assert.AreEqual(value2, retrievedValueForKey2BeforeRemove,
                    "Retrieved value for key 2 before removing it is not the expected.");
                Assert.IsTrue(cache.ContainsKey(key2), 
                    "ContainsKey returned unexpected result for key 2 before removing it.");
                var retrievedValueForKey2AfterRemove = (string)cache.Get(key2);
                cache.Remove(key2);
                Assert.AreEqual(value2, retrievedValueForKey2AfterRemove,
                    "Retrieved value for key 2 after removing it should be null.");
                Assert.IsFalse(cache.ContainsKey(key2),
                    "ContainsKey returned unexpected result for key 2 after removing it.");

                // Overwriting a value
                var key3 = "key_3";
                var value3 = "value_3";
                cache.Insert(key3, value3);
                var retrievedValueForKey3BeforeOverwrite = (string)cache.Get(key3);
                Assert.AreEqual(value3, retrievedValueForKey3BeforeOverwrite,
                    "Retrieved value for key 3 before overwriting it is not the expected.");
                Assert.IsTrue(cache.ContainsKey(key3),
                    "ContainsKey returned unexpected result for key 3 before overwriting it.");
                var newValue3 = "value_3_new";
                cache.Insert(key3, newValue3);
                var retrievedValueForKey3AfterOverwrite = (string)cache.Get(key3);
                Assert.AreEqual(newValue3, retrievedValueForKey3AfterOverwrite,
                    "Retrieved value for key 3 after overwriting it is not the expected.");
                Assert.IsTrue(cache.ContainsKey(key3),
                    "ContainsKey returned unexpected result for key 3 after overwriting it.");

                // I clear the cache
                cache.Clear();

                Assert.IsEmpty(cache.AllKeys(), "Cache should contain no keys after clearing it.");
            }

        }
    }
}
