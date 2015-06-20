using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Helpers
{
    public class AppSettingsHelperTest
    {
        private NameValueCollection _appSettings = new NameValueCollection();
        private IAppSettingsHelper _helper;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _appSettings.Add("5", "5");
            _appSettings.Add("5.5", "5.5");
            _appSettings.Add("1.23456", "1.23456");
            _appSettings.Add("true", "true");
            _appSettings.Add("false", "false");
            _appSettings.Add("true1", "True");
            _appSettings.Add("false1", "False");
            _appSettings.Add("true2", "TrUe");
            _appSettings.Add("false2", "FaLsE");
            _appSettings.Add("timespan", "1.23:34:45.6789");
            _appSettings.Add("timespan_bad_hours", "1.25:34:45.6789");
            _appSettings.Add("timespan_bad_minutes", "1.23:61:45.6789");
            _appSettings.Add("timespan_bad_seconds", "1.23:34:61.6789");
            _appSettings.Add("goodGuid", "728fc126-5cfb-4729-ba07-0078a0fade4f");
            _appSettings.Add("badGuid", "728fc126-5cfb-4729-ba07-0078a0fade4x");
            _appSettings.Add("string", "string");
            _helper = new AppSettingsHelper(_appSettings, new ParseHelper());
        }

        [Test]
        public void GetInt_ForNonExistingtKey_ReturnsNull()
        {
            // Arrange
            var key = "non existing key";

            // Act
            var result = _helper.GetInt(key);

            // Assert
            Assert.IsFalse(result.HasValue, "The result should be null.");
        }

        [Test]
        public void GetInt_ForKeyWithIntegerValue_ReturnsTheInteger()
        {
            // Arrange
            var key = "5";
            var expected = 5;

            // Act
            var result = _helper.GetInt(key);

            // Assert
            Assert.IsTrue(result.HasValue, "The result should have a value.");
            Assert.AreEqual(expected, result, "The result has the wrong value.");
        }

        [Test]
        public void GetInt_ForKeyWithFloatValue_ReturnsNull()
        {
            // Arrange
            var key = "5.5";

            // Act
            var result = _helper.GetInt(key);

            // Assert
            Assert.IsFalse(result.HasValue, "The result should be null.");
        }

        [Test]
        public void GetInt_ForKeyWithNonNumericValue_ReturnsNull()
        {
            // Arrange
            var key = "string";

            // Act
            var result = _helper.GetInt(key);

            // Assert
            Assert.IsFalse(result.HasValue, "The result should be null.");
        }

        [Test]
        public void GetLong_ForKeyWithLongNumber_ReturnsTheLong()
        {
            // Arrange
            long expected = 223372036854775808;
            string value = "223372036854775808";
            var key = value;
            _appSettings.Add(key, value);

            // Act
            var result = _helper.GetLong(key);

            // Assert
            Assert.IsTrue(result.HasValue, "The result should have a value.");
            Assert.AreEqual(expected, result, "The result has the wrong value.");
        }

        [Test]
        public void GetLong_ForKeyWithFloatValue_ReturnsNull()
        {
            // Arrange
            var key = "5.5";

            // Act
            var result = _helper.GetLong(key);

            // Assert
            Assert.IsFalse(result.HasValue, "The result should be null.");
        }

        [Test]
        public void GetLong_ForKeyWithNonNumericValue_ReturnsNull()
        {
            // Arrange
            var key = "string";

            // Act
            var result = _helper.GetLong(key);

            // Assert
            Assert.IsFalse(result.HasValue, "The result should be null.");
        }

        [Test]
        public void GetFloat_ForNonExistingtKey_ReturnsNull()
        {
            // Arrange
            var key = "non existing key";

            // Act
            var result = _helper.GetFloat(key);

            // Assert
            Assert.IsFalse(result.HasValue, "The result should be null.");
        }

        [Test]
        public void GetFloat_ForKeyWithIntegerValue_ReturnsTheIntegerAsFloat()
        {
            // Arrange
            var key = "5";
            var expected = 5.0F;

            // Act
            var result = _helper.GetFloat(key);

            // Assert
            Assert.IsTrue(result.HasValue, "The result should have a value.");
            Assert.AreEqual(expected, result.Value, "The result has the wrong value.");
        }

        [Test]
        public void GetFloat_ForKeyWithFloatValue_ReturnsTheFloat()
        {
            // Arrange
            var key = "5.5";
            var expected = 5.5F;

            // Act
            var result = _helper.GetFloat(key);

            // Assert
            Assert.IsTrue(result.HasValue, "The result should have a value.");
            Assert.AreEqual(expected, result.Value, "The result has the wrong value.");
        }

        [Test]
        public void GetFloat_ForKeyWithNonNumericValue_ReturnsNull()
        {
            // Arrange
            var key = "string";

            // Act
            var result = _helper.GetFloat(key);

            // Assert
            Assert.IsFalse(result.HasValue, "The result should be null.");
        }

        [Test]
        public void GetDouble_ForKeyWithIntegerValue_ReturnsTheIntegerAsDouble()
        {
            // Arrange
            var key = "5";
            var expected = 5.0;

            // Act
            var result = _helper.GetDouble(key);

            // Assert
            Assert.IsTrue(result.HasValue, "The result should have a value.");
            Assert.AreEqual(expected, result.Value, "The result has the wrong value.");
        }

        [Test]
        public void GetDouble_ForKeyWithDoubleValue_ReturnsTheDouble()
        {
            // Arrange
            var key = "5.5";
            var expected = 5.5;

            // Act
            var result = _helper.GetDouble(key);

            // Assert
            Assert.IsTrue(result.HasValue, "The result should have a value.");
            Assert.AreEqual(expected, result.Value, "The result has the wrong value.");
        }

        [Test]
        public void GetDouble_ForKeyWithNonNumericValue_ReturnsNull()
        {
            // Arrange
            var key = "string";

            // Act
            var result = _helper.GetDouble(key);

            // Assert
            Assert.IsFalse(result.HasValue, "The result should be null.");
        }

        [Test]
        public void GetDecimal_ForKeyWithIntegerValue_ReturnsTheIntegerAsDecimal()
        {
            // Arrange
            var key = "5";
            var expected = 5.0M;

            // Act
            var result = _helper.GetDecimal(key);

            // Assert
            Assert.IsTrue(result.HasValue, "The result should have a value.");
            Assert.AreEqual(expected, result.Value, "The result has the wrong value.");
        }

        [Test]
        public void GetDecimal_ForKeyWithFloatValue_ReturnsTheFloatAsDecimal()
        {
            // Arrange
            var key = "1.23456";
            var expected = 1.23456M;

            // Act
            var result = _helper.GetDecimal(key);

            // Assert
            Assert.IsTrue(result.HasValue, "The result should have a value.");
            Assert.AreEqual(expected, result.Value, "The result has the wrong value.");
        }

        [Test]
        public void GetDecimal_ForKeyWithNonNumericValue_ReturnsNull()
        {
            // Arrange
            var key = "string";

            // Act
            var result = _helper.GetDecimal(key);

            // Assert
            Assert.IsFalse(result.HasValue, "The result should be null.");
        }

        [Test]
        public void GetBool_ForKeyWithTrueValue_ReturnsTrue()
        {
            // Arrange
            var key1 = "true";
            var key2 = "true1";
            var key3 = "true2";

            // Act
            var result1 = _helper.GetBool(key1);
            var result2 = _helper.GetBool(key2);
            var result3 = _helper.GetBool(key3);

            // Assert
            Assert.IsTrue(result1.HasValue, "result1 should have a value.");
            Assert.IsTrue(result2.HasValue, "result2 should have a value.");
            Assert.IsTrue(result3.HasValue, "result3 should have a value.");
            Assert.AreEqual(true, result1.Value, "result1 has wrong value.");
            Assert.AreEqual(true, result2.Value, "result2 has wrong value.");
            Assert.AreEqual(true, result3.Value, "result3 has wrong value.");
        }

        [Test]
        public void GetBool_ForKeyWithFalseValue_ReturnsTrue()
        {
            // Arrange
            var key1 = "false";
            var key2 = "false1";
            var key3 = "false2";

            // Act
            var result1 = _helper.GetBool(key1);
            var result2 = _helper.GetBool(key2);
            var result3 = _helper.GetBool(key3);

            // Assert
            Assert.IsTrue(result1.HasValue, "result1 should have a value.");
            Assert.IsTrue(result2.HasValue, "result2 should have a value.");
            Assert.IsTrue(result3.HasValue, "result3 should have a value.");
            Assert.AreEqual(false, result1.Value, "result1 has wrong value.");
            Assert.AreEqual(false, result2.Value, "result2 has wrong value.");
            Assert.AreEqual(false, result3.Value, "result3 has wrong value.");
        }

        [Test]
        public void GetBool_ForKeyWithNonBooleanValue_ReturnsNull()
        {
            // Arrange
            var key = "string";

            // Act
            var result = _helper.GetBool(key);

            // Assert
            Assert.IsFalse(result.HasValue, "The result should be null.");
        }

        [Test]
        public void GetTimeSpan_ForNonExistingKey_ReturnsNull()
        {
            // Arrange
            var key = "non-existing key";

            // Act
            var result = _helper.GetTimeSpan(key);

            // Assert
            Assert.IsNull(result, "The result should be null.");
        }

        [Test]
        public void GetTimeSpan_ForKeyWithTimeSpanValue_ReturnsTheTimeSpan()
        {
            // Arrange
            var key = "timespan";

            // Act
            var result = _helper.GetTimeSpan(key);

            // Assert
            Assert.IsTrue(result.HasValue, "The result should have a value.");
            Assert.AreEqual(1, result.Value.Days, "Days in value are not the expected.");
            Assert.AreEqual(23, result.Value.Hours, "Hours in result are not the expected.");
            Assert.AreEqual(34, result.Value.Minutes, "Minutes in result are not the expected.");
            Assert.AreEqual(45, result.Value.Seconds, "Seconds in result are not the expected.");
            Assert.AreEqual(678, result.Value.Milliseconds, "Milliseconds in result are not the expected.");
        }

        [Test]
        public void GetTimeSpan_ForKeyWithNonTimeSpanValue_ReturnsNull()
        {
            // Arrange
            var key = "string";

            // Act
            var result = _helper.GetTimeSpan(key);

            // Assert
            Assert.IsFalse(result.HasValue, "The result should be null.");
        }

        [Test]
        public void GetTimeSpan_WithOutOfRangeHours_ReturnsNull()
        {
            // Arrange
            var key = "timespan_bad_hours";

            // Act
            var result = _helper.GetTimeSpan(key);

            // Assert
            Assert.IsFalse(result.HasValue, "The result should be null.");
        }

        [Test]
        public void GetTimeSpan_WithOutOfRangeMinutes_ReturnsNull()
        {
            // Arrange
            var key = "timespan_bad_minutes";

            // Act
            var result = _helper.GetTimeSpan(key);

            // Assert
            Assert.IsFalse(result.HasValue, "The result should be null.");
        }

        [Test]
        public void GetTimeSpan_WithOutOfRangeSeconds_ReturnsNull()
        {
            // Arrange
            var key = "timespan_bad_seconds";

            // Act
            var result = _helper.GetTimeSpan(key);

            // Assert
            Assert.IsFalse(result.HasValue, "The result should be null.");
        }

        // TODO: DateTime

        // TODO: DateTimeOffset

        [Test]
        public void GetGuid_ForNonExistingKey_ReturnsNull()
        {
            // Arrange
            var key = "non-existing key";

            // Act
            var result = _helper.GetGuid(key);

            // Assert
            Assert.IsNull(result, "The result should be null.");
        }

        [Test]
        public void GetGuid_ForGoodGuid_ReturnsTheGuid()
        {
            // Arrange
            var key = "goodGuid";
            var expected = _appSettings.Get(key).ToLower();

            // Act
            var result = _helper.GetGuid(key);

            // Assert
            Assert.IsTrue(result.HasValue, "The result should have a value.");
            var actual = result.ToString().ToLower();
            Assert.AreEqual(expected, actual, "The Guid is not the expected one.");
        }

        [Test]
        public void GetGuid_ForBadGuid_ReturnsNull()
        {
            // Arrange
            var key = "badGuid";

            // Act
            var result = _helper.GetGuid(key);

            // Assert
            Assert.IsNull(result, "The result should be null.");
        }

        [Test]
        public void GetString_ForNonExistingKey_ReturnsNull()
        {
            // Arrange
            var key = "non-existing key";

            // Act
            var result = _helper.GetString(key);

            // Assert
            Assert.IsNull(result, "The result should be null.");
        }

        [Test]
        public void GetString_ForExistingKey_ReturnsTheValue()
        {
            // Arrange
            var key = "string";
            var expected = "string";

            // Act
            var result = _helper.GetString(key);

            // Assert
            Assert.AreEqual(expected, result, "The result should be the value for the key.");
        }

        [Test]
        public void OptionalSettingHasValue_ForMatchingValue_ReturnsTrueIgnoringCase()
        {
            // Arrange
            var key1 = "true";
            var key2 = "true1";
            var key3 = "true2";
            var value = "tRuE";

            // Act
            var result1 = _helper.OptionalSettingHasValue(key1, value);
            var result2 = _helper.OptionalSettingHasValue(key2, value);
            var result3 = _helper.OptionalSettingHasValue(key3, value);

            // Assert
            Assert.AreEqual(true, result1, "result1 should be true.");
            Assert.AreEqual(true, result2, "result2 should be true.");
            Assert.AreEqual(true, result3, "result3 should be true.");
        }

        [Test]
        public void OptionalSettingHasValue_ForNonMatchingValue_ReturnsFalse()
        {
            // Arrange
            var key1 = "true";
            var key2 = "true1";
            var key3 = "true2";
            var value = "tru";

            // Act
            var result1 = _helper.OptionalSettingHasValue(key1, value);
            var result2 = _helper.OptionalSettingHasValue(key2, value);
            var result3 = _helper.OptionalSettingHasValue(key3, value);

            // Assert
            Assert.AreEqual(false, result1, "result1 should be false.");
            Assert.AreEqual(false, result2, "result2 should be false.");
            Assert.AreEqual(false, result3, "result3 should be false.");
        }

        [Test]
        public void OptionalSettingHasValue_ForMissingKey_DoesNotMatchEmptyString()
        {
            // Arrange
            var key = "non-existing key";
            var value = "";

            // Act
            var result = _helper.OptionalSettingHasValue(key, value);

            // Assert
            Assert.AreEqual(false, result, "The result should be false.");
        }

        public void AllSettings_GivesAllTheSettings()
        {
            // Arrange
            var appSettings = new NameValueCollection();
            var key1 = "key1";
            var value1 = "value1";
            var key2 = "key2";
            var value2 = "value2";
            var appSettingsHelper = new AppSettingsHelper(appSettings, new ParseHelper());

            // Act
            var allSettings = appSettingsHelper.AllSettings();
            var setting1 = allSettings.SingleOrDefault(x => x.Value == key1);
            var setting2 = allSettings.SingleOrDefault(x => x.Value == key2);

            // Assert
            Assert.AreEqual(2, allSettings.Count(), "AllSettings has the wrong size.");
            Assert.AreEqual(value1, setting1, "Setting for key2 has wrong value.");
            Assert.AreEqual(value2, setting2, "Setting for key1 has wrong value.");
        }
    }
}