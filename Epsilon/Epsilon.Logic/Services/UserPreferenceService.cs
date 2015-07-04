using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class UserPreferenceService : IUserPreferenceService
    {
        private readonly IAppCache _appCache;
        private readonly IEpsilonContext _dbContext;

        public UserPreferenceService(
            IAppCache appCache,
            IEpsilonContext dbContext)
        {
            _appCache = appCache;
            _dbContext = dbContext;
        }

        public async Task CreateUserPreference(string userId, string languageId)
        {
            var newUserPreference = new UserPreference
            {
                Id = userId,
                LanguageId = languageId.ToLower()
            };
            _dbContext.UserPreferences.Add(newUserPreference);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserPreference> GetUserPreference(string userId)
        {
            var userPreference = await _appCache
                .Get(AppCacheKey.UserPreference(userId), () => _dbContext.UserPreferences.FindAsync(userId), WithLock.No);
            return userPreference;
        }
    }
}
