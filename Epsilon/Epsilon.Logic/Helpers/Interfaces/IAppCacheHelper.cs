namespace Epsilon.Logic.Helpers.Interfaces
{
    public interface IAppCacheHelper
    {
        void RemoveCachedUserSubmissionsSummary(string userId);

        void RemoveCachedUserOutgoingVerificationsSummary(string userId);

        void RemoveCachedUserExploredPropertiesSummary(string userId);
    }
}
