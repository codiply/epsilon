using Epsilon.Logic.Wrappers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Wrappers
{
    [TestFixture]
    public class RandomWrapperTest
    {
        [Test]
        public void RandomCanBeSeeded()
        {
            var seed = 2015;
            var numberOfRandoms = 100;
            var random1 = new RandomWrapper(seed);
            var randomSequence1 = Enumerable.Range(1, numberOfRandoms).Select(x => random1.NextDouble()).ToArray();
            var random2 = new RandomWrapper(seed);
            var randomSequence2 = Enumerable.Range(1, numberOfRandoms).Select(x => random2.NextDouble()).ToArray();

            for (var i = 0; i < numberOfRandoms; i++)
            {
                Assert.AreEqual(randomSequence1[i], randomSequence2[i],
                    string.Format("Test failed at position {0}.", i));
            }
        }

        [Test]
        public void RandomNextGivesAnswerInTheExpectedRange()
        {
            var numberOfTries = 10000;
            var minValue = 10;
            var maxValueExclusive = 20;
            var random = new RandomWrapper();
            for (var i = 0; i < numberOfTries; i++)
            {
                var randomValue = random.Next(minValue, maxValueExclusive);
                Assert.That(randomValue, Is.GreaterThanOrEqualTo(minValue),
                    string.Format("Test failed for i equal to {0}", i));
                Assert.That(randomValue, Is.LessThan(maxValueExclusive),
                    string.Format("Test failed for i equal to {0}", i));
            }
        }

        [Test]
        public void RandomNextDoubleGivesAnswerInTheExpectedRange()
        {
            // I do no use a seed here, in order to test the unseeded random.
            var numberOfTries = 10000;
            var minValue = 0.0;
            var maxValueExclusive = 1.0;
            var random = new RandomWrapper();
            for (var i = 0; i < numberOfTries; i++)
            {
                var randomValue = random.NextDouble();
                Assert.That(randomValue, Is.GreaterThanOrEqualTo(minValue),
                    string.Format("Test failed for i equal to {0}", i));
                Assert.That(randomValue, Is.LessThan(maxValueExclusive),
                    string.Format("Test failed for i equal to {0}", i));
            }
        }

        [Test]
        public void RandomPickTest()
        {
            // I just test that it works here and that the two options are more or less picked with equal weights.
            var option1 = "option1";
            var option2 = "option2";
            var options = new List<string> { option1, option2 }.ToArray();
            var numberOfTries = 1000000;
            var seed = 2015;
            var random = new RandomWrapper(seed);

            var choices = Enumerable.Range(1, numberOfTries).Select(x => random.Pick(options));
            var option1Count = choices.Count(x => x.Equals(option1));
            var option2Count = choices.Count(x => x.Equals(option2));
            var difference = Math.Abs(option1Count - option2Count);

            Assert.That(difference, Is.LessThan(Math.Sqrt(numberOfTries)));
        }
    }
}
