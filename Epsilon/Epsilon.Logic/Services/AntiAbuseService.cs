﻿using Epsilon.Logic.Constants;
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
        private readonly IAdminEventLogService _adminEventLogService;
        private readonly IEpsilonContext _dbContext;

        public AntiAbuseService(
            IClock clock,
            IAntiAbuseServiceConfig antiAbuseServiceConfig,
            IAdminAlertService adminAlertService,
            IAdminEventLogService adminEventLogService,
            IEpsilonContext dbContext)
        {
            _clock = clock;
            _antiAbuseServiceConfig = antiAbuseServiceConfig;
            _adminAlertService = adminAlertService;
            _adminEventLogService = adminEventLogService;
            _dbContext = dbContext;
        }

        public async Task<AntiAbuseServiceResponse> CanRegister(string userIpAddress)
        {
            if (_antiAbuseServiceConfig.GlobalSwitch_DisableRegister)
                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.GlobalSwitch_RegisterDisabled_Message
                };

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
            var checkGlobalFrequency = await CanAddAddressCheckGlobalFrequency();
            if (checkGlobalFrequency.IsRejected)
                return checkGlobalFrequency;

            var checkIpFrequency = await CanAddAddressCheckIpFrequency(userIpAddress);
            if (checkIpFrequency.IsRejected)
                return checkIpFrequency;

            var checkUserFrequency = await CanAddAddressCheckUserFrequency(userId);
            if (checkUserFrequency.IsRejected)
                return checkUserFrequency;

            var checkGeocodeFailureIpFrequency = await CanAddAddressCheckGeocodeFailureIpFrequency(userIpAddress);
            if (checkGeocodeFailureIpFrequency.IsRejected)
                return checkGeocodeFailureIpFrequency;

            var checkGeocodeFailureUserFrequency = await CanAddAddressCheckGeocodeFailureUserFrequency(userId);
            if (checkGeocodeFailureUserFrequency.IsRejected)
                return checkGeocodeFailureUserFrequency;

            return new AntiAbuseServiceResponse { IsRejected = false };
        }

        public async Task<AntiAbuseServiceResponse> CanCreateTenancyDetailsSubmission(string userId, string userIpAddress)
        {
            var checkGlobalFrequency = await CanCreateTenancyDetailsSubmissionCheckGlobalFrequency();
            if (checkGlobalFrequency.IsRejected)
                return checkGlobalFrequency;

            var checkIpFrequency = await CanCreateTenancyDetailsSubmissionCheckIpFrequency(userIpAddress);
            if (checkIpFrequency.IsRejected)
                return checkIpFrequency;

            var checkUserFrequency = await CanCreateTenancyDetailsSubmissionCheckUserFrequency(userId);
            if (checkUserFrequency.IsRejected)
                return checkUserFrequency;

            return new AntiAbuseServiceResponse { IsRejected = false };
        }

        public async Task<AntiAbuseServiceResponse> CanPickOutgoingVerification(string userId, string userIpAddress)
        {
            // TODO_PANOS_TEST
            var checkGlobalFrequency = await CanPickOutgoingVerificationCheckGlobalFrequency();
            if (checkGlobalFrequency.IsRejected)
                return checkGlobalFrequency;

            // TODO_PANOS_TEST
            var checkIpFrequency = await CanPickOutgoingVerificationCheckIpFrequency(userIpAddress);
            if (checkIpFrequency.IsRejected)
                return checkIpFrequency;

            // TODO_PANOS_TEST
            var checkOutstandingOutgoingVerifications = await CanPickOutgoingVerificationCheckMaxOutstandingForUser(userId);
            if (checkOutstandingOutgoingVerifications.IsRejected)
                return checkOutstandingOutgoingVerifications;

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
                await _adminEventLogService.Log(AdminEventLogKey.RegistrationGlobalMaxFrequencyReached, null);

                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.Register_GlobalFrequencyCheck_RejectionMessage
                };
            }
            return new AntiAbuseServiceResponse { IsRejected = false };
        }

        private async Task<AntiAbuseServiceResponse> CanAddAddressCheckGlobalFrequency()
        {
            if (_antiAbuseServiceConfig.AddAddress_DisableGlobalFrequencyCheck)
                return new AntiAbuseServiceResponse { IsRejected = false };

            var maxFrequency = _antiAbuseServiceConfig.AddAddress_GlobalMaxFrequency;

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.Addresses
                .Where(a => a.CreatedOn > windowStart)
                .CountAsync();

            if (actualTimes >= maxFrequency.Times)
            {
                _adminAlertService.SendAlert(AdminAlertKey.AddAddressGlobalMaxFrequencyReached);
                await _adminEventLogService.Log(AdminEventLogKey.AddAddressGlobalMaxFrequencyReached, null);

                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.AddAddress_GlobalFrequencyCheck_RejectionMessage
                };
            }
            return new AntiAbuseServiceResponse { IsRejected = false };
        }

        private async Task<AntiAbuseServiceResponse> CanCreateTenancyDetailsSubmissionCheckGlobalFrequency()
        {
            if (_antiAbuseServiceConfig.CreateTenancyDetailsSubmission_DisableGlobalFrequencyCheck)
                return new AntiAbuseServiceResponse { IsRejected = false };

            var maxFrequency = _antiAbuseServiceConfig.CreateTenancyDetailsSubmission_GlobalMaxFrequency;

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.TenancyDetailsSubmissions
                .Where(a => a.CreatedOn > windowStart)
                .CountAsync();

            if (actualTimes >= maxFrequency.Times)
            {
                _adminAlertService.SendAlert(AdminAlertKey.CreateTenancyDetailsSubmissionGlobalMaxFrequencyReached);
                await _adminEventLogService.Log(AdminEventLogKey.CreateTenancyDetailsSubmissionGlobalMaxFrequencyReached, null);

                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.CreateTenancyDetailsSubmission_GlobalFrequencyCheck_RejectionMessage
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

        private async Task<AntiAbuseServiceResponse> CanAddAddressCheckGeocodeFailureIpFrequency(string ipAddress)
        {
            if (_antiAbuseServiceConfig.AddAddress_DisableGeocodeFailureIpAddressFrequencyCheck)
                return new AntiAbuseServiceResponse { IsRejected = false };

            var maxFrequency = _antiAbuseServiceConfig.AddAddress_MaxGeocodeFailureFrequencyPerIpAddress;

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.GeocodeFailures
                .Where(a => a.CreatedByIpAddress.Equals(ipAddress))
                .Where(a => a.CreatedOn > windowStart)
                .CountAsync();

            if (actualTimes >= maxFrequency.Times)
                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.AddAddress_GeocodeFailureIpAddressFrequencyCheck_RejectionMessage
                };

            return new AntiAbuseServiceResponse { IsRejected = false };
        }

        private async Task<AntiAbuseServiceResponse> CanAddAddressCheckGeocodeFailureUserFrequency(string userId)
        {
            if (_antiAbuseServiceConfig.AddAddress_DisableGeocodeFailureUserFrequencyCheck)
                return new AntiAbuseServiceResponse { IsRejected = false };

            var maxFrequency = _antiAbuseServiceConfig.AddAddress_MaxGeocodeFailureFrequencyPerUser;

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.GeocodeFailures
                .Where(a => a.CreatedById.Equals(userId))
                .Where(a => a.CreatedOn > windowStart)
                .CountAsync();

            if (actualTimes >= maxFrequency.Times)
                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.AddAddress_GeocodeFailureUserFrequencyCheck_RejectionMessage
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

        private async Task<AntiAbuseServiceResponse> CanPickOutgoingVerificationCheckGlobalFrequency()
        {
            if (_antiAbuseServiceConfig.PickOutgoingVerification_DisableGlobalFrequencyCheck)
                return new AntiAbuseServiceResponse { IsRejected = false };

            var maxFrequency = _antiAbuseServiceConfig.PickOutgoingVerification_GlobalMaxFrequency;

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.TenantVerifications
                .Where(v => v.CreatedOn > windowStart)
                .CountAsync();

            if (actualTimes >= maxFrequency.Times)
            {
                // TODO_PANOS_TEST
                _adminAlertService.SendAlert(AdminAlertKey.PickOutgoingVerificationGlobalMaxFrequencyReached);
                // TODO_PANOS_TEST
                await _adminEventLogService.Log(AdminEventLogKey.PickOutgoingVerificationGlobalMaxFrequencyReached, null);

                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.PickOutgoingVerification_GlobalFrequencyCheck_RejectionMessage
                };
            }
            return new AntiAbuseServiceResponse { IsRejected = false };
        }

        private async Task<AntiAbuseServiceResponse> CanPickOutgoingVerificationCheckIpFrequency(string ipAddress)
        {
            if (_antiAbuseServiceConfig.PickOutgoingVerification_DisableIpAddressFrequencyCheck)
                return new AntiAbuseServiceResponse { IsRejected = false };

            // TODO_PANOS_TEST
            var maxFrequency = _antiAbuseServiceConfig.PickOutgoingVerification_MaxFrequencyPerIpAddress;

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.TenantVerifications
                .Where(v => v.AssignedByIpAddress.Equals(ipAddress))
                .Where(v => v.CreatedOn > windowStart)
                .CountAsync();

            if (actualTimes >= maxFrequency.Times)
                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.PickOutgoingVerification_IpAddressFrequencyCheck_RejectionMessage
                };

            return new AntiAbuseServiceResponse { IsRejected = false };
        }

        private async Task<AntiAbuseServiceResponse> CanPickOutgoingVerificationCheckMaxOutstandingForUser(string userId)
        {
            if (_antiAbuseServiceConfig.PickOutgoingVerification_DisableMaxOutstandingPerUserCheck)
                return new AntiAbuseServiceResponse { IsRejected = false };

            // TODO_PANOS_TEST
            var numberOfCompleteVerifications = await _dbContext.TenantVerifications
                .Where(v => v.AssignedToId.Equals(userId))
                .Where(v => !v.VerifiedOn.HasValue)
                .LongCountAsync();

            var maxOutstandingVerifications =
                _antiAbuseServiceConfig.PickOutgoingVerification_MaxOutstandingPerUserConstant + numberOfCompleteVerifications;

            var numberOfOutstandingVerifications = await _dbContext.TenantVerifications
                .Where(v => v.AssignedToId.Equals(userId))
                .Where(v => !v.VerifiedOn.HasValue)
                .CountAsync();

            if (numberOfOutstandingVerifications >= maxOutstandingVerifications)
            {
                return new AntiAbuseServiceResponse
                {
                    IsRejected = true,
                    RejectionReason = AntiAbuseResources.PickOutgoingVerification_MaxOutstandingPerUserCheck_RejectionMessage
                };
            }

            return new AntiAbuseServiceResponse { IsRejected = false };
        }
    }
}
