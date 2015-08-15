using Epsilon.Logic.Helpers;
using Epsilon.Logic.Wrappers;
using NUnit.Framework;
using System;
using System.Text.RegularExpressions;

namespace Epsilon.UnitTests.Logic.Helpers
{
    [TestFixture]
    public class RandomStringHelperTest
    {
        private const int numberOfTests = 100;
        private const int stringLength = 10;

        [Test]
        public void GetStringTest_LowerCase()
        {
            var random = new RandomWrapper();

            var regex = new Regex(@"^[a-z]{" + stringLength +"}$");

            for (int i = 0; i < numberOfTests; i++)
            {
                var answer = RandomStringHelper.GetString(random, stringLength, RandomStringHelper.CharacterCase.Lower);
                var match = regex.Match(answer);
                Assert.IsTrue(match.Success, String.Format("Case '{0}' did not match pattern '{1}'", 
                    answer, regex.ToString()));
            }
        }

        [Test]
        public void GetStringTest_MixedCase()
        {
            var random = new RandomWrapper();

            var regex = new Regex(@"^[a-zA-Z]{" + stringLength + "}$");

            for (int i = 0; i < numberOfTests; i++)
            {
                var answer = RandomStringHelper.GetString(random, stringLength, RandomStringHelper.CharacterCase.Mixed);
                var match = regex.Match(answer);
                Assert.IsTrue(match.Success, String.Format("Case '{0}' did not match pattern '{1}'",
                    answer, regex.ToString()));
            }
        }

        [Test]
        public void GetStringTest_UpperCase()
        {
            var random = new RandomWrapper();

            var regex = new Regex(@"^[A-Z]{" + stringLength + "}$");

            for (int i = 0; i < numberOfTests; i++)
            {
                var answer = RandomStringHelper.GetString(random, stringLength, RandomStringHelper.CharacterCase.Upper);
                var match = regex.Match(answer);
                Assert.IsTrue(match.Success, String.Format("Case '{0}' did not match pattern '{1}'",
                    answer, regex.ToString()));
            }
        }

        [Test]
        public void GetAlphaNumericStringTest_LowerCase()
        {
            var random = new RandomWrapper();

            var regex = new Regex(@"^[0-9a-z]{" + stringLength + "}$");

            for (int i = 0; i < numberOfTests; i++)
            {
                var answer = RandomStringHelper.GetAlphaNumericString(random, stringLength, 
                    RandomStringHelper.CharacterCase.Lower);
                var match = regex.Match(answer);
                Assert.IsTrue(match.Success, String.Format("Case '{0}' did not match pattern '{1}'",
                    answer, regex.ToString()));
            }
        }

        [Test]
        public void GetAlphaNumericStringTest_MixedCase()
        {
            var random = new RandomWrapper();

            var regex = new Regex(@"^[0-9a-zA-Z]{" + stringLength + "}$");

            for (int i = 0; i < numberOfTests; i++)
            {
                var answer = RandomStringHelper.GetAlphaNumericString(random, stringLength, 
                    RandomStringHelper.CharacterCase.Mixed);
                var match = regex.Match(answer);
                Assert.IsTrue(match.Success, String.Format("Case '{0}' did not match pattern '{1}'",
                    answer, regex.ToString()));
            }
        }

        [Test]
        public void GetAlphaNumericStringTest_UpperCase()
        {
            var random = new RandomWrapper();

            var regex = new Regex(@"^[0-9A-Z]{" + stringLength + "}$");

            for (int i = 0; i < numberOfTests; i++)
            {
                var answer = RandomStringHelper.GetAlphaNumericString(random, stringLength, 
                    RandomStringHelper.CharacterCase.Upper);
                var match = regex.Match(answer);
                Assert.IsTrue(match.Success, String.Format("Case '{0}' did not match pattern '{1}'",
                    answer, regex.ToString()));
            }
        }


        [Test]
        public void GetDigitStringTest()
        {
            var random = new RandomWrapper();

            var regex = new Regex(@"^[0-9]{" + stringLength + "}$");

            for (int i = 0; i < numberOfTests; i++)
            {
                var answer = RandomStringHelper.GetDigitString(random, stringLength);
                var match = regex.Match(answer);
                Assert.IsTrue(match.Success, String.Format("Case '{0}' did not match pattern '{1}'",
                    answer, regex.ToString()));
            }
        }
    }
}
