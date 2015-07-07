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
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Configuration.Interfaces;

namespace Epsilon.Logic.Services
{
    public class AntiAbuseService : IAntiAbuseService
    {
        private readonly IClock _clock;
        private readonly IAntiAbuseServiceConfig _antiAbuseServiceConfig;
        private readonly IAdminAlertService _adminAlertService;
        private readonly IEpsilonContext _dbContext;

        public AntiAbuseService(
            IClock clock,
            IAntiAbuseServiceConfig antiAbuseServiceConfig,
            IAdminAlertService adminAlertService,
            IEpsilonContext dbContext)
        {
            _clock = clock;
            _antiAbuseServiceConfig = antiAbuseServiceConfig;
            _adminAlertService = adminAlertService;
            _dbContext = dbContext;
        }

        public async Task<AntiAbuseServiceResponse> CanRegister(string userIpAddress)
        {
            var checkGlobalFrequency = await CanRegisterCheckGlobalFrequency();
            if (checkGlobalFrequency.IsRejected)
                return checkGlobalFrequency;

            var checkIpFrequency = await CanRegisterCheckIpAddressFrequency(userIpAddress);
            if (checkIpFrequency.IsRejected)
                return checkIpFrequency;

            return new AntiAbuseServiceResponse { IsRejected = false };
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

        private async Task<AntiAbuseServiceResponse> CanRegisterCheckGlobalFrequency()
        {
            if (_antiAbuseServiceConfig.Register_DisableGlobalFrequencyCheck)
                return new AntiAbuseServiceResponse { IsRejected = false };

            var maxFrequency = _antiAbuseServiceConfig.Register_GlobalMaxFrequency;

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var activityType = EnumsHelper.IpAddressActivityType.ToString(IpAddressActivityType.Registration);
            var actualTimes = await _dbContext.IpAddressActivities
                .Where(a => a.ActivityType.Equals(activityType))
                .Where(a => a.RecordedOn > windowStart)
                .CountAsync();

            if (actualTimes >= maxFrequency.Times)
            {
                _adminAlertService.SendAlert(AdminAlertKey.RegistrationGlobalMaxFrequencyReached);

                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.Register_GlobalFrequencyCheck_RejectionMessage
                };
            }
            return new AntiAbuseServiceResponse { IsRejected = false };
        }

        private async Task<AntiAbuseServiceResponse> CanRegisterCheckIpAddressFrequency(string ipAddress)
        {
            if (_antiAbuseServiceConfig.Register_DisableIpAddressFrequencyCheck)
                return new AntiAbuseServiceResponse { IsRejected = false };

            var maxFrequency = _antiAbuseServiceConfig.Register_MaxFrequencyPerIpAddress;

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var activityType = EnumsHelper.IpAddressActivityType.ToString(IpAddressActivityType.Registration);
            var actualTimes = await _dbContext.IpAddressActivities
                .Where(a => a.ActivityType.Equals(activityType))
                .Where(a => a.IpAddress.Equals(ipAddress))
                .Where(a => a.RecordedOn > windowStart)
                .CountAsync();

            if (actualTimes >= maxFrequency.Times)
                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.Register_IpAddressFrequencyCheck_RejectionMessage
                };

            return new AntiAbuseServiceResponse { IsRejected = false };
        }
        
        private async Task<AntiAbuseServiceResponse> CanAddAddressCheckIpFrequency(string ipAddress)
        {
            if (_antiAbuseServiceConfig.AddAddress_DisableIpAddressFrequencyCheck)
                return new AntiAbuseServiceResponse { IsRejected = false };

            var maxFrequency = _antiAbuseServiceConfig.AddAddress_MaxFrequencyPerIpAddress;

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.Addresses
                .Where(a => a.CreatedByIpAddress.Equals(ipAddress))
                .Where(a => a.CreatedOn > windowStart)
                .CountAsync();

            if (actualTimes >= maxFrequency.Times)
                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.AddAddress_IpAddressFrequencyCheck_RejectionMessage
                };

            return new AntiAbuseServiceResponse { IsRejected = false };
        }

        private async Task<AntiAbuseServiceResponse> CanAddAddressCheckUserFrequency(string userId)
        {
            if (_antiAbuseServiceConfig.AddAddress_DisableUserFrequencyCheck)
                return new AntiAbuseServiceResponse { IsRejected = false };

            var maxFrequency = _antiAbuseServiceConfig.AddAddress_MaxFrequencyPerUser;

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.Addresses
                .Where(a => a.CreatedById.Equals(userId))
                .Where(a => a.CreatedOn > windowStart)
                .CountAsync();

            if (actualTimes >= maxFrequency.Times)
                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.AddAddress_UserFrequencyCheck_RejectionMessage
                };

            return new AntiAbuseServiceResponse { IsRejected = false };
        }

        private async Task<AntiAbuseServiceResponse> CanCreateTenancyDetailsSubmissionCheckIpFrequency(string ipAddress)
        {
            if (_antiAbuseServiceConfig.CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck)
                return new AntiAbuseServiceResponse { IsRejected = false };

            var maxFrequency = _antiAbuseServiceConfig.CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress;

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.TenancyDetailsSubmissions
                .Where(a => a.CreatedByIpAddress.Equals(ipAddress))
                .Where(a => a.CreatedOn > windowStart)
                .CountAsync();

            if (actualTimes >= maxFrequency.Times)
                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.CreateTenancyDetailsSubmission_IpAddressFrequencyCheck_RejectionMessage
                };

            return new AntiAbuseServiceResponse { IsRejected = false };
        }

        private async Task<AntiAbuseServiceResponse> CanCreateTenancyDetailsSubmissionCheckUserFrequency(string userId)
        {
            if (_antiAbuseServiceConfig.CreateTenancyDetailsSubmission_DisableUserFrequencyCheck)
                return new AntiAbuseServiceResponse { IsRejected = false };

            var maxFrequency = _antiAbuseServiceConfig.CreateTenancyDetailsSubmission_MaxFrequencyPerUser;

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.TenancyDetailsSubmissions
                .Where(a => a.UserId.Equals(userId))
                .Where(a => a.CreatedOn > windowStart)
                .CountAsync();

            if (actualTimes >= maxFrequency.Times)
                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.CreateTenancyDetailsSubmission_UserFrequencyCheck_RejectionMessage
                };

            return new AntiAbuseServiceResponse { IsRejected = false };
        }
    }
}
