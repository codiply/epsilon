﻿using Epsilon.Logic.Helpers;
using Epsilon.Logic.SqlContext.Mapping;
using NUnit.Framework;

namespace Epsilon.UnitTests.Logic.Constants.Enums
{
    [TestFixture]
    public class DbAppSettingKeyTest
    {
        [Test]
        public void MaxLengthTest()
        {
            foreach (var key in EnumsHelper.DbAppSettingKey.GetNames())
            {
                Assert.That(key.Length, Is.LessThanOrEqualTo(AppSettingMap.ID_MAX_LENGTH),
                    string.Format("The key '{0}' has length greater than the maximum length.", key));
            }
        }
    }
}
