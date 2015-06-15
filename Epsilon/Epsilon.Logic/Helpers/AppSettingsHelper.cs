using Epsilon.Logic.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers
{
    public class AppSettingsHelper : IAppSettingsHelper
    {
        private NameValueCollection _appSettings;
        private readonly IParseHelper _parseHelper;

        public AppSettingsHelper(
            NameValueCollection appSettings,
            IParseHelper parseHelper)
        {
            _appSettings = appSettings;
            _parseHelper = parseHelper;
        }

        protected NameValueCollection Collection
        {
            get { return _appSettings; }
            set { _appSettings = value; }
        }

        public int? GetInt(string key)
        {
            string value = _appSettings[key];

            return _parseHelper.ParseInt(value);
        }

        public int GetInt(string key, int defaultValue)
        {
            var value = GetInt(key);

            return value.HasValue ? value.Value : defaultValue;
        }

        public long? GetLong(string key)
        {
            string value = _appSettings[key];

            return _parseHelper.ParseLong(value);
        }

        public long GetLong(string key, long defaultValue)
        {
            var value = GetLong(key);

            return value.HasValue ? value.Value : defaultValue;
        }

        public float? GetFloat(string key)
        {
            string value = _appSettings[key];

            return _parseHelper.ParseFloat(value);
        }

        public float GetFloat(string key, float defaultValue)
        {
            var value = GetFloat(key);

            return value.HasValue ? value.Value : defaultValue;
        }

        public double? GetDouble(string key)
        {
            string value = _appSettings[key];

            return _parseHelper.ParseDouble(value);
        }

        public double GetDouble(string key, double defaultValue)
        {
            var value = GetDouble(key);

            return value.HasValue ? value.Value : defaultValue;
        }

        public decimal? GetDecimal(string key)
        {
            string value = _appSettings[key];

            return _parseHelper.ParseDecimal(value);
        }

        public decimal GetDecimal(string key, decimal defaultValue)
        {
            var value = GetDecimal(key);

            return value.HasValue ? value.Value : defaultValue;
        }


        public bool? GetBool(string key)
        {
            string value = _appSettings[key];

            return _parseHelper.ParseBool(value);
        }

        public bool GetBool(string key, bool defaultValue)
        {
            var value = GetBool(key);

            return value.HasValue ? value.Value : defaultValue;
        }

        public TimeSpan? GetTimeSpan(string key)
        {
            string value = _appSettings[key];

            return _parseHelper.ParseTimeSpan(value);
        }

        public TimeSpan GetTimeSpan(string key, TimeSpan defaultValue)
        {
            var value = GetTimeSpan(key);

            return value.HasValue ? value.Value : defaultValue;
        }

        public DateTime? GetDateTime(string key)
        {
            string value = _appSettings[key];

            return _parseHelper.ParseDateTime(value);
        }

        public DateTime GetDateTime(string key, DateTime defaultValue)
        {
            var value = GetDateTime(key);

            return value.HasValue ? value.Value : defaultValue;
        }

        public DateTimeOffset? GetDateTimeOffset(string key)
        {
            string value = _appSettings[key];

            return _parseHelper.ParseDateTimeOffset(value);
        }

        public DateTimeOffset GetDateTimeOffset(string key, DateTimeOffset defaultValue)
        {
            var value = GetDateTimeOffset(key);

            return value.HasValue ? value.Value : defaultValue;
        }

        public Guid? GetGuid(string key)
        {
            string value = _appSettings[key];

            return _parseHelper.ParseGuid(value);
        }

        public string GetString(string key)
        {
            string value = _appSettings[key];

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return null;
        }

        public bool OptionalSettingHasValue(string key, string value)
        {
            string actualValue = _appSettings[key];

            if (!string.IsNullOrWhiteSpace(actualValue) &&
                actualValue.ToLower() == value.ToLower())
            {
                return true;
            }

            return false;
        }

        public IEnumerable<KeyValuePair<string, string>> AllSettings()
        {
            return _appSettings.AllKeys
                .Select(key => new KeyValuePair<string, string>(key, _appSettings[key])).ToList();
        }
    }
}
