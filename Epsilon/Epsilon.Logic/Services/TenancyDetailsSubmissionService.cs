﻿using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Resources.Logic.TenancyDetailsSubmission;
using Epsilon.Resources.Web.Submission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Epsilon.Logic.Services
{
    public class TenancyDetailsSubmissionService : ITenancyDetailsSubmissionService
    {
        private readonly IClock _clock;
        private readonly ITenancyDetailsSubmissionServiceConfig _tenancyDetailsSubmissionServiceConfig;
        private readonly IEpsilonContext _dbContext;
        private readonly IAddressService _addressService;
        private readonly IAntiAbuseService _antiAbuseService;

        public TenancyDetailsSubmissionService(
            IClock clock,
            ITenancyDetailsSubmissionServiceConfig tenancyDetailsSubmissionServiceConfig,
            IEpsilonContext dbContext,
            IAddressService addressService,
            IAntiAbuseService antiAbuseService)
        {
            _clock = clock;
            _tenancyDetailsSubmissionServiceConfig = tenancyDetailsSubmissionServiceConfig;
            _dbContext = dbContext;
            _addressService = addressService;
            _antiAbuseService = antiAbuseService;
        }

        public async Task<CreateTenancyDetailsSubmissionOutcome> Create(
            string userId, 
            string userIpAddress, 
            Guid submissionId,
            Guid addressId)
        {
            var address = await _addressService.GetAddress(addressId);
            if (address == null)
            {
                return new CreateTenancyDetailsSubmissionOutcome
                {
                    IsRejected = true,
                    RejectionReason = SubmissionResources.UseAddressConfirmed_AddressNotFoundMessage
                };
            }

            var antiAbuseCheck = await _antiAbuseService.CanCreateTenancyDetailsSubmission(userId, userIpAddress);
            if (antiAbuseCheck.IsRejected)
            {
                return new CreateTenancyDetailsSubmissionOutcome
                {
                    IsRejected = true,
                    RejectionReason = antiAbuseCheck.RejectionReason
                };
            }

            if (!_tenancyDetailsSubmissionServiceConfig.Create_DisableFrequencyPerAddressCheck)
            {
                var recentSubmissionsExist = await TooManyRecentSubmissionsExist(addressId);
                if (recentSubmissionsExist)
                    return new CreateTenancyDetailsSubmissionOutcome
                    {
                        IsRejected = true,
                        RejectionReason = TenancyDetailsSubmissionResources.Create_MaxFrequencyPerAddressCheck_RejectionMessage
                    };
            }

            var tenancyDetailsSubmission = await DoCreate(userId, userIpAddress, submissionId, addressId);
            return new CreateTenancyDetailsSubmissionOutcome
            {
                IsRejected = false,
                TenancyDetailsSubmissionId = tenancyDetailsSubmission.Id
            };
        }

        private async Task<bool> TooManyRecentSubmissionsExist(Guid addressId)
        {
            var maxFrequency = _tenancyDetailsSubmissionServiceConfig.Create_MaxFrequencyPerAddress;

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.TenancyDetailsSubmissions
                .Where(s => s.AddressId == addressId)
                .Where(a => a.CreatedOn > windowStart)
                .CountAsync();

            return actualTimes >= maxFrequency.Times;
        }

        private async Task<TenancyDetailsSubmission> DoCreate(
            string userId,
            string userIpAddress,
            Guid submissionId,
            Guid addressId)
        {
            var entity = new TenancyDetailsSubmission
            {
                Id = submissionId,
                UserId = userId,
                CreatedByIpAddress = userIpAddress,
                AddressId = addressId
            };
            _dbContext.TenancyDetailsSubmissions.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
    }
}
