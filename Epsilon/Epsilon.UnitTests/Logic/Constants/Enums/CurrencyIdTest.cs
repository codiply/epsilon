using Epsilon.Logic.Helpers;
using NUnit.Framework;
using System.Linq;

namespace Epsilon.UnitTests.Logic.Constants.Enums
{
    [TestFixture]
    public class CurrencyIdTest
    {
        [Test]
        public void AllIdsAreAllUppercase()
        {
            foreach (var currencyId in EnumsHelper.CurrencyId.GetNames())
            {
                Assert.IsTrue(currencyId.All(char.IsUpper),
                    string.Format("CurrencyId '{0}' is not all uppercase.", currencyId));
            }
        }
    }
}
