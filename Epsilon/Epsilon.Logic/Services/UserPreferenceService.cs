using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class UserPreferenceService : IUserPreferenceService
    {
        private readonly IClock _clock;
        private readonly IAppCache _appCache;
        private readonly IEpsilonContext _dbContext;

        public UserPreferenceService(
            IClock clock,
            IAppCache appCache,
            IEpsilonContext dbContext)
        {
            _clock = clock;
            _appCache = appCache;
            _dbContext = dbContext;
        }

        public async Task Create(string userId, string languageId)
        {
            var newUserPreference = new UserPreference
            {
                Id = userId,
                LanguageId = languageId.ToLower(),
                UpdatedOn = _clock.OffsetNow // TODO_PANOS_TEST
            };
            _dbContext.UserPreferences.Add(newUserPreference);
            await _dbContext.SaveChangesAsync();
        }

        public UserPreference Get(string userId)
        {
            return Task.Run(() => GetAsync(userId)).Result;
        }

        public async Task<UserPreference> GetAsync(string userId)
        {
            var userPreference = await _appCache
                .Get(AppCacheKey.UserPreference(userId), () => _dbContext.UserPreferences.FindAsync(userId), WithLock.Yes);
            return userPreference;
        }
    }
}
