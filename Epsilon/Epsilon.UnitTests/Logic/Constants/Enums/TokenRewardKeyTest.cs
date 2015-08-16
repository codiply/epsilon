using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers;
using Epsilon.Resources.Logic.TokenRewardKey;
using NUnit.Framework;

namespace Epsilon.UnitTests.Logic.Constants.Enums
{
    [TestFixture]
    public class TokenRewardKeyTest
    {
        [Test]
        public void AllKeysStartWithEitherEarnOrSpend()
        {
            var allKeys = EnumsHelper.TokenRewardKey.GetNames();

            foreach (var key in allKeys)
            {
                var passesTest = key.StartsWith(AppConstant.TOKEN_REWARD_KEY_EARN) || key.StartsWith(AppConstant.TOKEN_REWARD_KEY_SPEND);
                Assert.IsTrue(passesTest, 
                    string.Format("Key '{0}' doesn't start with either '{1}' or '{2}'.", 
                    key, AppConstant.TOKEN_REWARD_KEY_EARN, AppConstant.TOKEN_REWARD_KEY_SPEND));
            }
        }

        [Test]
        public void DisplayNameAndDescriptionIsDefinedForAllTokenRewardKeys()
        {
            var allKeys = EnumsHelper.TokenRewardKey.GetNames();

            foreach (var key in allKeys)
            {
                var displayNameResourceName = key + "_DisplayName";
                var displayName = TokenRewardKeyResources.ResourceManager.GetString(displayNameResourceName);
                Assert.IsNotNullOrEmpty(displayName,
                    string.Format("Resource '{0}' was not found in TokenRewardKeyResources.", displayNameResourceName));


                var descriptionResourceName = key + "_Description";
                var descriptionName = TokenRewardKeyResources.ResourceManager.GetString(descriptionResourceName);
                Assert.IsNotNullOrEmpty(descriptionName,
                    string.Format("Resource '{0}' was not found in TokenRewardKeyResources.", descriptionResourceName));

            }
        }
    }
}
