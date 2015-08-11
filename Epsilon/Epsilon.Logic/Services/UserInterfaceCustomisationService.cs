using Epsilon.Logic.Constants;
using Epsilon.Logic.Infrastructure.Interfaces;
using Epsilon.Logic.Models;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    // TODO_PANOS_TEST
    public class UserInterfaceCustomisationService : IUserInterfaceCustomisationService
    {
        private readonly IEpsilonContext _dbContext;
        private readonly IAppCache _appCache;
        private readonly IUserResidenceService _userResidenceService;
        private readonly IAntiAbuseService _antiAbuseService;

        public UserInterfaceCustomisationService(
            IEpsilonContext dbContext,
            IAppCache appCache,
            IUserResidenceService userResidenceService,
            IAntiAbuseService antiAbuseService)
        {
            _dbContext = dbContext;
            _appCache = appCache;
            _userResidenceService = userResidenceService;
            _antiAbuseService = antiAbuseService;
        }

        public UserInterfaceCustomisationModel GetForUser(string userId)
        {
            return Task.Run(() => GetForUserAsync(userId)).Result;
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
            var userResidenceResponse = await _userResidenceService.GetResidence(userId);

            var canCreateTenancyDetailsSubmissionOutcome = await _antiAbuseService.CanCreateTenancyDetailsSubmissionCheckUserFrequency(userId);

            return new UserInterfaceCustomisationModel()
            {
                HasNoTenancyDetailsSubmissions = userResidenceResponse.HasNoSubmissions,
                IsUserResidenceVerified = userResidenceResponse.IsVerified,
                UserResidenceCountry = userResidenceResponse.Address == null ? null : userResidenceResponse.Address.Country,
                CanCreateTenancyDetailsSubmission = !canCreateTenancyDetailsSubmissionOutcome.IsRejected,
                CanPickOutgoingVerification = userResidenceResponse.Address != null
            };
        }
    }
}
