using Epsilon.Logic.Infrastructure.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers.Interfaces
{
    public interface IAppSettingsHelper
    {
        int? GetInt(string key);

        int GetInt(string key, int defaultValue);

        long? GetLong(string key);

        long GetLong(string key, long defaultValue);

        float? GetFloat(string key);

        float GetFloat(string key, float defaultValue);

        double? GetDouble(string key);

        double GetDouble(string key, double defaultValue);

        decimal? GetDecimal(string key);

        decimal GetDecimal(string key, decimal defaultValue);   

        bool? GetBool(string key);

        bool GetBool(string key, bool defaultValue);

        TimeSpan? GetTimeSpan(string key);

        TimeSpan GetTimeSpan(string key, TimeSpan defaultValue);

        Frequency GetFrequency(string key);

        Frequency GetFrequency(string key, Frequency defaultValue);

        Guid? GetGuid(string key);

        string GetString(string key);

        bool OptionalSettingHasValue(string key, string value);

        IEnumerable<KeyValuePair<string, string>> AllSettings();
    }
}
