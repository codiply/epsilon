using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Interfaces;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Resources.Logic.AntiAbuse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Epsilon.Logic.Services
{
    public class AntiAbuseService : IAntiAbuseService
    {
        private readonly IClock _clock;
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly IDbAppSettingDefaultValue _dbAppSettingDefaultValue;
        private readonly IEpsilonContext _dbContext;

        public AntiAbuseService(
            IClock clock,
            IDbAppSettingsHelper dbAppSettingsHelper,
            IDbAppSettingDefaultValue dbAppSettingDefaultValue,
            IEpsilonContext dbContext)
        {
            _clock = clock;
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
            _dbContext = dbContext;
        }

        public async Task<AntiAbuseServiceResponse> CanAddAddress(string userId, string userIpAddress)
        {
            var checkIpFrequency = await CanAddAddressCheckIpFrequency(userIpAddress);
            if (checkIpFrequency.IsRejected)
                return checkIpFrequency;

            var checkUserFrequency = await CanAddAddressCheckUserFrequency(userId);
            if (checkUserFrequency.IsRejected)
                return checkUserFrequency;

            return new AntiAbuseServiceResponse { IsRejected = false };
        }

        public async Task<AntiAbuseServiceResponse> CanCreateTenancyDetailsSubmission(string userId, string userIpAddress)
        {
            var checkIpFrequency = await CanCreateTenancyDetailsSubmissionCheckIpFrequency(userIpAddress);
            if (checkIpFrequency.IsRejected)
                return checkIpFrequency;

            var checkUserFrequency = await CanCreateTenancyDetailsSubmissionCheckUserFrequency(userId);
            if (checkUserFrequency.IsRejected)
                return checkUserFrequency;

            return new AntiAbuseServiceResponse { IsRejected = false };
        }
        
        private async Task<AntiAbuseServiceResponse> CanAddAddressCheckIpFrequency(string ipAddress)
        {
            var maxFrequency = _dbAppSettingsHelper.GetFrequency(
                DbAppSettingKey.AntiAbuse_AddAddress_MaxFrequencyPerIpAddress,
                _dbAppSettingDefaultValue.AntiAbuse_AddAddress_MaxFrequencyPerIpAddress);

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.Addresses
                .Where(a => a.CreatedByIpAddress.Equals(ipAddress))
                .Where(a => a.CreatedOn > windowStart)
                .CountAsync();

            if (actualTimes > maxFrequency.Times)
                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.AddAddress_IpFrequencyCheck_RejectionMessage
                };

            return new AntiAbuseServiceResponse { IsRejected = false };
        }

        private async Task<AntiAbuseServiceResponse> CanAddAddressCheckUserFrequency(string userId)
        {
            var maxFrequency = _dbAppSettingsHelper.GetFrequency(
                DbAppSettingKey.AntiAbuse_AddAddress_MaxFrequencyPerUser,
                _dbAppSettingDefaultValue.AntiAbuse_AddAddress_MaxFrequencyPerUser);

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.Addresses
                .Where(a => a.CreatedById.Equals(userId))
                .Where(a => a.CreatedOn > windowStart)
                .CountAsync();

            if (actualTimes > maxFrequency.Times)
                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.AddAddress_UserFrequencyCheck_RejectionMessage
                };

            return new AntiAbuseServiceResponse { IsRejected = false };
        }

        private async Task<AntiAbuseServiceResponse> CanCreateTenancyDetailsSubmissionCheckIpFrequency(string ipAddress)
        {
            // Check IP address abuse
            var maxFrequency = _dbAppSettingsHelper.GetFrequency(
                DbAppSettingKey.AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress,
                _dbAppSettingDefaultValue.AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress);

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.TenancyDetailsSubmissions
                .Where(a => a.CreatedByIpAddress.Equals(ipAddress))
                .Where(a => a.CreatedOn > windowStart)
                .CountAsync();

            if (actualTimes > maxFrequency.Times)
                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.CreateTenancyDetailsSubmission_IpFrequencyCheck_RejectionMessage
                };

            return new AntiAbuseServiceResponse { IsRejected = false };
        }

        private async Task<AntiAbuseServiceResponse> CanCreateTenancyDetailsSubmissionCheckUserFrequency(string userId)
        {
            // Check user abuse
            var maxFrequency = _dbAppSettingsHelper.GetFrequency(
                DbAppSettingKey.AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser,
                _dbAppSettingDefaultValue.AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser);

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.TenancyDetailsSubmissions
                .Where(a => a.UserId.Equals(userId))
                .Where(a => a.CreatedOn > windowStart)
                .CountAsync();

            if (actualTimes > maxFrequency.Times)
                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.CreateTenancyDetailsSubmission_UserFrequencyCheck_RejectionMessage
                };

            return new AntiAbuseServiceResponse { IsRejected = false };
        }
    }
}
