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
    public class DbAppSettingValueTypeTest
    {
        [Test]
        public void MaxLengthTest()
        {
            foreach (var type in EnumsHelper.DbAppSettingValueType.GetNames())
            {
                Assert.That(type.Length, Is.LessThanOrEqualTo(AppSettingMap.VALUE_TYPE_MAX_LENGTH),
                    string.Format("The type '{0}' has length greater than the maximum length.", type));
            }
        }
    }
}
