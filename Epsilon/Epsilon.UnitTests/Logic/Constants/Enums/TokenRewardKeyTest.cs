using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
