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

namespace Epsilon.Logic.Services
{
    public class AntiAbuseService : IAntiAbuseService
    {
        private readonly IClock _clock;
        private readonly IAdminAlertService _adminAlertService;
        private readonly IDbAppSettingsHelper _dbAppSettingsHelper;
        private readonly IDbAppSettingDefaultValue _dbAppSettingDefaultValue;
        private readonly IEpsilonContext _dbContext;

        public AntiAbuseService(
            IClock clock,
            IAdminAlertService adminAlertService,
            IDbAppSettingsHelper dbAppSettingsHelper,
            IDbAppSettingDefaultValue dbAppSettingDefaultValue,
            IEpsilonContext dbContext)
        {
            _clock = clock;
            _adminAlertService = adminAlertService;
            _dbAppSettingsHelper = dbAppSettingsHelper;
            _dbAppSettingDefaultValue = dbAppSettingDefaultValue;
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
            if (_dbAppSettingsHelper
                .GetBool(DbAppSettingKey.AntiAbuse_Register_DisableGlobalFrequencyCheck) == true)
                return new AntiAbuseServiceResponse { IsRejected = false };

            var maxFrequency = _dbAppSettingsHelper.GetFrequency(
                DbAppSettingKey.AntiAbuse_Register_GlobalMaxFrequency,
                _dbAppSettingDefaultValue.AntiAbuse_Register_GlobalMaxFrequency);

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.IpAddressActivities
                .Where(a => a.ActivityType.Equals(IpAddressActivityType.Registration.ToString()))
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
            if (_dbAppSettingsHelper
                .GetBool(DbAppSettingKey.AntiAbuse_Register_DisableIpAddressFrequencyCheck) == true)
                return new AntiAbuseServiceResponse { IsRejected = false };

            var maxFrequency = _dbAppSettingsHelper.GetFrequency(
                DbAppSettingKey.AntiAbuse_Register_MaxFrequencyPerIpAddress,
                _dbAppSettingDefaultValue.AntiAbuse_Register_MaxFrequencyPerIpAddress);

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.IpAddressActivities
                .Where(a => a.ActivityType.Equals(IpAddressActivityType.Registration.ToString()))
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
            if (_dbAppSettingsHelper
                .GetBool(DbAppSettingKey.AntiAbuse_AddAddress_DisableIpAddressFrequencyCheck) == true)
                return new AntiAbuseServiceResponse { IsRejected = false };

            var maxFrequency = _dbAppSettingsHelper.GetFrequency(
                DbAppSettingKey.AntiAbuse_AddAddress_MaxFrequencyPerIpAddress,
                _dbAppSettingDefaultValue.AntiAbuse_AddAddress_MaxFrequencyPerIpAddress);

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
            if (_dbAppSettingsHelper
                .GetBool(DbAppSettingKey.AntiAbuse_AddAddress_DisableUserFrequencyCheck) == true)
                return new AntiAbuseServiceResponse { IsRejected = false };

            var maxFrequency = _dbAppSettingsHelper.GetFrequency(
                DbAppSettingKey.AntiAbuse_AddAddress_MaxFrequencyPerUser,
                _dbAppSettingDefaultValue.AntiAbuse_AddAddress_MaxFrequencyPerUser);

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
            if (_dbAppSettingsHelper
                .GetBool(DbAppSettingKey.AntiAbuse_CreateTenancyDetailsSubmission_DisableIpAddressFrequencyCheck) == true)
                return new AntiAbuseServiceResponse { IsRejected = false };

            var maxFrequency = _dbAppSettingsHelper.GetFrequency(
                DbAppSettingKey.AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress,
                _dbAppSettingDefaultValue.AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerIpAddress);

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
            if (_dbAppSettingsHelper
                .GetBool(DbAppSettingKey.AntiAbuse_CreateTenancyDetailsSubmission_DisableUserFrequencyCheck) == true)
                return new AntiAbuseServiceResponse { IsRejected = false };

            var maxFrequency = _dbAppSettingsHelper.GetFrequency(
                DbAppSettingKey.AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser,
                _dbAppSettingDefaultValue.AntiAbuse_CreateTenancyDetailsSubmission_MaxFrequencyPerUser);

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
