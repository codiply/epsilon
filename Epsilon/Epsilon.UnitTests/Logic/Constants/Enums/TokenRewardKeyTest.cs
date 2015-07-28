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
                var passesTest = key.StartsWith("Earn") || key.StartsWith("Spend");
                Assert.IsTrue(passesTest, string.Format("Key '{0}' doesn't start with either 'Earn' or 'Spend'.", key));
            }
        }
    }
}
