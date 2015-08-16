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
    public class CountryIdTest
    {
        [Test]
        public void AllIdsAreAllUppercase()
        {
            foreach (var countryId in EnumsHelper.CountryId.GetNames())
            {
                Assert.IsTrue(countryId.All(char.IsUpper),
                    string.Format("CountryId '{0}' is not all uppercase.", countryId));
            }
        }
    }
}
