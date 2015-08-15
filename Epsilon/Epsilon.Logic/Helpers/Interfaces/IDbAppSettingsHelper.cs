using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Forms.Admin;
using Epsilon.Logic.Infrastructure.Primitives;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers.Interfaces
{
    public interface IDbAppSettingsHelper : IAppSettingsHelper
    {
        Task<IList<AppSetting>> GetAllAppSettingEntities();

        Task<IList<AppSetting>> GetAllAppSettingEntitiesForLabel(string label);

        Task<IList<string>> GetAllLabels();

        Task<AppSetting> GetAppSettingEntity(string id);

        Task Update(DbAppSettingForm form, string userId);

        int? GetInt(DbAppSettingKey key);
        int GetInt(DbAppSettingKey key, int defaultValue);
        long? GetLong(DbAppSettingKey key);
        long GetLong(DbAppSettingKey key, long defaultValue);
        float? GetFloat(DbAppSettingKey key);
        float GetFloat(DbAppSettingKey key, float defaultValue);
        double? GetDouble(DbAppSettingKey key);
        double GetDouble(DbAppSettingKey key, double defaultValue);
        decimal? GetDecimal(DbAppSettingKey key);
        decimal GetDecimal(DbAppSettingKey key, decimal defaultValue);
        bool? GetBool(DbAppSettingKey key);
        bool GetBool(DbAppSettingKey key, bool defaultValue);
        TimeSpan? GetTimeSpan(DbAppSettingKey key);
        TimeSpan GetTimeSpan(DbAppSettingKey key, TimeSpan defaultValue);
        Frequency GetFrequency(DbAppSettingKey key);
        Frequency GetFrequency(DbAppSettingKey key, Frequency defaultValue);
        Guid? GetGuid(DbAppSettingKey key);
        string GetString(DbAppSettingKey key);
        bool OptionalSettingHasValue(DbAppSettingKey key, string value);
    }
}
