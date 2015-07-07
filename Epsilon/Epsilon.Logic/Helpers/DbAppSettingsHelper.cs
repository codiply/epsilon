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
            return await _dbContext.AppSettings.OrderBy(s => s.Id).ToListAsync();
        }

        public async Task<AppSetting> GetAppSettingEntity(string id)
        {
            return await _dbContext.AppSettings.FindAsync(id);
        }

        public async Task Update(DbAppSettingForm form, string userId)
        {
            var entity = await GetAppSettingEntity(form.Id);

            entity.Value = form.Value;
            entity.UpdatedById = userId;
            entity.UpdatedOn = _clock.OffsetNow;

            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            // Refresh cached values.
            _appCache.Remove(AppCacheKey.DB_APP_SETTINGS);
            PopulateCollection();
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
