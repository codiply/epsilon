using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Forms.Manage;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Resources.Common;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class UserPreferenceService : IUserPreferenceService
    {
        private readonly IClock _clock;
        private readonly IAppCache _appCache;
        private readonly IEpsilonContext _dbContext;
        private readonly ILanguageService _languageService;

        public UserPreferenceService(
            IClock clock,
            IAppCache appCache,
            IEpsilonContext dbContext,
            ILanguageService languageService)
        {
            _clock = clock;
            _appCache = appCache;
            _dbContext = dbContext;
            _languageService = languageService;
        }

        public async Task Create(string userId, string languageId)
        {
            var newUserPreference = new UserPreference
            {
                Id = userId,
                LanguageId = languageId.ToLower(),
                UpdatedOn = _clock.OffsetNow
            };
            _dbContext.UserPreferences.Add(newUserPreference);
            await _dbContext.SaveChangesAsync();
        }

        public UserPreference Get(string userId, bool allowCaching = true)
        {
            return Task.Run(async () => await GetAsync(userId, allowCaching)).Result;
        }

        public async Task<UserPreference> GetAsync(string userId, bool allowCaching = true)
        {
            var query = _dbContext.UserPreferences.Include(x => x.Language);

            if (allowCaching)
            {
                var userPreference = await _appCache
                    .Get(AppCacheKey.UserPreference(userId), () => query.SingleOrDefaultAsync(x => x.Id.Equals(userId)), WithLock.Yes);
                return userPreference;
            }
            else
            {
                var userPreference = await query.SingleOrDefaultAsync(x => x.Id.Equals(userId));
                return userPreference;
            }
        }

        public async Task<ChangePreferencesOutcome> ChangePreferences(string userId, ChangePreferencesForm form)
        {
            // Check the selected language is valid
            var selectedLanguage = _languageService.GetLanguage(form.LanguageId);
            if (selectedLanguage == null || !selectedLanguage.IsAvailable)
            {
                return new ChangePreferencesOutcome
                {
                    IsSuccess = false,
                    ErrorMessage = CommonResources.GenericInvalidRequestMessage,
                    ReturnToForm = false
                };
            }

            var userPreference = await _dbContext.UserPreferences.FindAsync(userId);
            if (userPreference == null)
            {
                return new ChangePreferencesOutcome
                {
                    IsSuccess = false,
                    ErrorMessage = CommonResources.GenericInvalidRequestMessage,
                    ReturnToForm = false
                };
            }

            userPreference.LanguageId = form.LanguageId;
            userPreference.UpdatedOn = _clock.OffsetNow;
            _dbContext.Entry(userPreference).State = EntityState.Modified;
            var result = await _dbContext.SaveChangesAsync();

            // Remove cached preference from cache for specific user.
            _appCache.Remove(AppCacheKey.UserPreference(userId));

            return new ChangePreferencesOutcome
            {
                IsSuccess = true
            };
        }
    }
}
