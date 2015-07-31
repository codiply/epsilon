using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Epsilon.Logic.Forms;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Logic.Forms.Admin;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Infrastructure.Primitives;

namespace Epsilon.Logic.Helpers
{
    public class DbAppSettingsHelper : AppSettingsHelper, IDbAppSettingsHelper
    {
        private readonly IClock _clock;
        private readonly IEpsilonContext _dbContext;
        private readonly IAppCache _appCache;

        public DbAppSettingsHelper(
            IClock clock,
            IEpsilonContext dbContext,
            IParseHelper parseHelper,
            IAppCache appCache)
            : base(null, parseHelper)
        {
            _clock = clock;
            _dbContext = dbContext;
            _appCache = appCache;
            PopulateCollection();
        }

        public async Task<IList<AppSetting>> GetAllAppSettingEntities()
        {
            return await _dbContext.AppSettings
                .Include(x => x.Labels)
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        public async Task<IList<AppSetting>> GetAllAppSettingEntitiesForLabel(string label)
        {
            return await _dbContext.AppSettings
                .Include(x => x.Labels)
                .Where(x => x.Labels.Any(l => l.Label.Equals(label)))
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        public async Task<IList<string>> GetAllLabels()
        {
            var allLabels = await _dbContext.AppSettingLabels
                .Select(x => x.Label)
                .Distinct()
                .ToListAsync();
            allLabels = allLabels.OrderBy(x => x).ToList();
            return allLabels;
        }

        public async Task<AppSetting> GetAppSettingEntity(string id)
        {
            return await _dbContext.AppSettings
                .Include(x => x.Labels)
                .SingleOrDefaultAsync(x => x.Id.Equals(id));
        }

        public async Task Update(DbAppSettingForm form, string userId)
        {
            var entity = await GetAppSettingEntity(form.Id);

            entity.Value = form.Value;
            entity.UpdatedById = userId;
            entity.UpdatedOn = _clock.OffsetNow;

            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            // I refresh cached values.
            _appCache.Remove(AppCacheKey.DB_APP_SETTINGS);
            PopulateCollection();
        }

        public int? GetInt(DbAppSettingKey key)
        {
            return GetInt(EnumsHelper.DbAppSettingKey.ToString(key));
        }

        public int GetInt(DbAppSettingKey key, int defaultValue)
        {
            return GetInt(EnumsHelper.DbAppSettingKey.ToString(key), defaultValue);
        }

        public long? GetLong(DbAppSettingKey key)
        {
            return GetLong(EnumsHelper.DbAppSettingKey.ToString(key));
        }

        public long GetLong(DbAppSettingKey key, long defaultValue)
        {
            return GetLong(EnumsHelper.DbAppSettingKey.ToString(key), defaultValue);
        }

        public float? GetFloat(DbAppSettingKey key)
        {
            return GetFloat(EnumsHelper.DbAppSettingKey.ToString(key));
        }

        public float GetFloat(DbAppSettingKey key, float defaultValue)
        {
            return GetFloat(EnumsHelper.DbAppSettingKey.ToString(key), defaultValue);
        }

        public double? GetDouble(DbAppSettingKey key)
        {
            return GetDouble(EnumsHelper.DbAppSettingKey.ToString(key));
        }

        public double GetDouble(DbAppSettingKey key, double defaultValue)
        {
            return GetDouble(EnumsHelper.DbAppSettingKey.ToString(key), defaultValue);
        }

        public decimal? GetDecimal(DbAppSettingKey key)
        {
            return GetDecimal(EnumsHelper.DbAppSettingKey.ToString(key));
        }

        public decimal GetDecimal(DbAppSettingKey key, decimal defaultValue)
        {
            return GetDecimal(EnumsHelper.DbAppSettingKey.ToString(key), defaultValue);
        }

        public bool? GetBool(DbAppSettingKey key)
        {
            return GetBool(EnumsHelper.DbAppSettingKey.ToString(key));
        }

        public bool GetBool(DbAppSettingKey key, bool defaultValue)
        {
            return GetBool(EnumsHelper.DbAppSettingKey.ToString(key), defaultValue);
        }

        public TimeSpan? GetTimeSpan(DbAppSettingKey key)
        {
            return GetTimeSpan(EnumsHelper.DbAppSettingKey.ToString(key));
        }

        public TimeSpan GetTimeSpan(DbAppSettingKey key, TimeSpan defaultValue)
        {
            return GetTimeSpan(EnumsHelper.DbAppSettingKey.ToString(key), defaultValue);
        }

        public Frequency GetFrequency(DbAppSettingKey key)
        {
            return GetFrequency(EnumsHelper.DbAppSettingKey.ToString(key));
        }

        public Frequency GetFrequency(DbAppSettingKey key, Frequency defaultValue)
        {
            return GetFrequency(EnumsHelper.DbAppSettingKey.ToString(key), defaultValue);
        }

        public Guid? GetGuid(DbAppSettingKey key)
        {
            return GetGuid(EnumsHelper.DbAppSettingKey.ToString(key));
        }

        public string GetString(DbAppSettingKey key)
        {
            return GetString(EnumsHelper.DbAppSettingKey.ToString(key));
        }

        private void PopulateCollection()
        {
            Collection = _appCache.Get<NameValueCollection>(AppCacheKey.DB_APP_SETTINGS, () =>
            {
                var allValues = _dbContext.AppSettings.OrderBy(x => x.Id).ToList();
                var collection = new NameValueCollection(allValues.Count);
                allValues.ForEach(x => collection.Add(x.Id, x.Value));
                return collection;
            }, WithLock.Yes);
        }
    }
}
