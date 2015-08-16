using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using Epsilon.Resources.Logic.TokenRewardKey;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Helpers
{
    [TestFixture]
    public class TokenRewardMetadataHelperTest
    {
        [Test]
        public void GetAll_Test()
        {
            var helper = new TokenRewardMetadataHelper();

            var resourceManager = TokenRewardKeyResources.ResourceManager;

            var allMetadata = helper.GetAll().ToDictionary(x => x.key);

            foreach (var key in EnumsHelper.TokenRewardKey.GetNames())
            {
                Assert.IsTrue(allMetadata.ContainsKey(key),
                    string.Format("Metadata for TokenRewardKey '{0}' was not found", key));

                var metadata = allMetadata[key];

                var expectedDisplayName = resourceManager.GetString(key + "_DisplayName");
                var expectedDescription = resourceManager.GetString(key + "_Description");

                Assert.AreEqual(expectedDisplayName, metadata.displayName);
                Assert.AreEqual(expectedDescription, metadata.description);
            }
        }

        [Test]
        public void ResourceNameForDisplayName_Test()
        {
            var helper = new TokenRewardMetadataHelper();
            var key = EnumsHelper.TokenRewardKey.GetValues().First();
            var expected = EnumsHelper.TokenRewardKey.ToString(key) + "_DisplayName";
            var actual = TokenRewardMetadataHelper.ResourceNameForDisplayName(key);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ResourceNameForDescription_Test()
        {
            var helper = new TokenRewardMetadataHelper();
            var key = EnumsHelper.TokenRewardKey.GetValues().First();
            var expected = EnumsHelper.TokenRewardKey.ToString(key) + "_Description";
            var actual = TokenRewardMetadataHelper.ResourceNameForDescription(key);
            Assert.AreEqual(expected, actual);
        }
    }
}
