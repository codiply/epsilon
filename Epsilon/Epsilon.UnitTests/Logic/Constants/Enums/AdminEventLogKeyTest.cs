using Epsilon.Logic.Helpers;
using Epsilon.Logic.SqlContext.Mapping;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Constants.Enums
{
    [TestFixture]
    public class AdminEventLogKeyTest
    {
        [Test]
        public void MaxLengthTest()
        {
            foreach (var key in EnumsHelper.AdminEventLogKey.GetNames())
            {
                Assert.That(key.Length, Is.LessThanOrEqualTo(AdminEventLogMap.KEY_MAX_LENGTH),
                    string.Format("The key '{0}' has length greater than the maximum length.", key));
            }
        }
    }
}
