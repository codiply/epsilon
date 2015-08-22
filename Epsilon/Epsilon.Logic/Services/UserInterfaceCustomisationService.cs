using Epsilon.Logic.Constants;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Models;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class UserInterfaceCustomisationService : IUserInterfaceCustomisationService
    {
        private readonly IAppCache _appCache;
        private readonly IUserResidenceService _userResidenceService;
        private readonly IAntiAbuseService _antiAbuseService;

        public UserInterfaceCustomisationService(
            IAppCache appCache,
            IUserResidenceService userResidenceService,
            IAntiAbuseService antiAbuseService)
        {
            _appCache = appCache;
            _userResidenceService = userResidenceService;
            _antiAbuseService = antiAbuseService;
        }

        public UserInterfaceCustomisationModel GetForUser(string userId)
        {
            return Task.Run(async () => await GetForUserAsync(userId)).Result;
        }

        public async Task<UserInterfaceCustomisationModel> GetForUserAsync(string userId)
        {
            return await _appCache.GetAsync(AppCacheKey.GetUserInterfaceCustomisationForUser(userId),
                async () => await DoGetForUser(userId), WithLock.No);
        }

        public void ClearCachedCustomisationForUser(string userId)
        {
            _appCache.Remove(AppCacheKey.GetUserInterfaceCustomisationForUser(userId));
        }

        private async Task<UserInterfaceCustomisationModel> DoGetForUser(string userId)
        {
            var userResidenceServiceResponse = await _userResidenceService.GetResidence(userId);

            var canCreateTenancyDetailsSubmissionResponse = await _antiAbuseService.CanCreateTenancyDetailsSubmissionCheckUserFrequency(userId);

            return new UserInterfaceCustomisationModel()
            {
                HasNoTenancyDetailsSubmissions = userResidenceServiceResponse.HasNoSubmissions,
                IsUserResidenceVerified = userResidenceServiceResponse.IsVerified,
                UserResidenceCountry = userResidenceServiceResponse.Address == null ? null : userResidenceServiceResponse.Address.Country,
                CanCreateTenancyDetailsSubmission = canCreateTenancyDetailsSubmissionResponse.IsRejected == false,
                CanPickOutgoingVerification = userResidenceServiceResponse.Address != null
            };
        }
    }
}
