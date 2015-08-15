using Epsilon.Logic.Infrastructure;
using NUnit.Framework;
using System.Linq;

namespace Epsilon.UnitTests.Logic.Infrastructure
{
    [TestFixture]
    public class EnumMemoizerTest
    {
        public enum TestEnum { Option1, Option2, Option3 }

        private EnumMemoizer<TestEnum> _enumMemoizer = new EnumMemoizer<TestEnum>();

        [Test]
        public void GetValuesTest()
        {
            var values = _enumMemoizer.GetValues().ToList();

            Assert.AreEqual(3, values.Count, "The number of values is not the expected");
            Assert.IsTrue(values.Contains(TestEnum.Option1), "Option1 was not found");
            Assert.IsTrue(values.Contains(TestEnum.Option2), "Option2 was not found");
            Assert.IsTrue(values.Contains(TestEnum.Option3), "Option3 was not found");
        }

        [Test]
        public void GetNamesTest()
        {
            var names = _enumMemoizer.GetNames().ToList();

            Assert.AreEqual(3, names.Count, "The number of names is not the expected");
            Assert.IsTrue(names.Contains("Option1"), "Option1 was not found");
            Assert.IsTrue(names.Contains("Option2"), "Option2 was not found");
            Assert.IsTrue(names.Contains("Option3"), "Option3 was not found");
        }

        [Test]
        public void ParseTest()
        {
            var parsedOption1 = _enumMemoizer.Parse("Option1");
            var parsedOption2 = _enumMemoizer.Parse("option2");
            var parsedOption3 = _enumMemoizer.Parse("oPtIoN3");
            var notParsed = _enumMemoizer.Parse("NOTPARSED");

            Assert.IsTrue(parsedOption1.HasValue, "Parsing Option1 should return a value");
            Assert.AreEqual(parsedOption1.Value, TestEnum.Option1, "Option1 was parsed to the wrong value.");

            Assert.IsTrue(parsedOption2.HasValue, "Parsing Option2 should return a value");
            Assert.AreEqual(parsedOption2.Value, TestEnum.Option2, "Option2 was parsed to the wrong value.");

            Assert.IsTrue(parsedOption3.HasValue, "Parsing Option3 should return a value");
            Assert.AreEqual(parsedOption3.Value, TestEnum.Option3, "Option3 was parsed to the wrong value.");

            Assert.IsFalse(notParsed.HasValue, "Non-existing option should return null when parsed.");
        }

        [Test]
        public void ToStringTest()
        {
            var option1ToString = _enumMemoizer.ToString(TestEnum.Option1);
            var option2ToString = _enumMemoizer.ToString(TestEnum.Option2);
            var option3ToString = _enumMemoizer.ToString(TestEnum.Option3);

            Assert.AreEqual(option1ToString, "Option1", "ToString on Option1 returned unexpected result.");
            Assert.AreEqual(option2ToString, "Option2", "ToString on Option2 returned unexpected result.");
            Assert.AreEqual(option3ToString, "Option3", "ToString on Option3 returned unexpected result.");
        }
    }
}
