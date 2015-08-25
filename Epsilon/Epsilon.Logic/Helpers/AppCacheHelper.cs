using Epsilon.Logic.Constants;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Interfaces;

namespace Epsilon.Logic.Helpers
{
    public class AppCacheHelper : IAppCacheHelper
    {
        private readonly IAppCache _appCache;

        public AppCacheHelper(
            IAppCache appCache)
        {
            _appCache = appCache;
        }

        public void RemoveCachedUserSubmissionsSummary(string userId)
        {
            _appCache.Remove(AppCacheKey.GetUserSubmissionsSummary(userId, true));
            _appCache.Remove(AppCacheKey.GetUserSubmissionsSummary(userId, false));
        }

        public void RemoveCachedUserOutgoingVerificationsSummary(string userId)
        {
            _appCache.Remove(AppCacheKey.GetUserOutgoingVerificationsSummary(userId, true));
            _appCache.Remove(AppCacheKey.GetUserOutgoingVerificationsSummary(userId, false));
        }

        public void RemoveCachedUserExploredPropertiesSummary(string userId)
        {
            _appCache.Remove(AppCacheKey.GetUserExploredPropertiesSummary(userId, true));
            _appCache.Remove(AppCacheKey.GetUserExploredPropertiesSummary(userId, false));
        }
    }
}
