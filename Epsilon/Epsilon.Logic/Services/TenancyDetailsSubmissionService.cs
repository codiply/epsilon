using Epsilon.Logic.Configuration.Interfaces;
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
using Epsilon.Logic.JsonModels;

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
            Guid submissionUniqueId,
            Guid addressUniqueId)
        {
            var address = await _addressService.GetAddress(addressUniqueId);
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
                var tooManyRecentSubmissionsExist = await TooManyRecentSubmissionsExist(address.Id);
                if (tooManyRecentSubmissionsExist)
                    return new CreateTenancyDetailsSubmissionOutcome
                    {
                        IsRejected = true,
                        RejectionReason = TenancyDetailsSubmissionResources.Create_MaxFrequencyPerAddressCheck_RejectionMessage
                    };
            }

            var tenancyDetailsSubmission = await DoCreate(userId, userIpAddress, submissionUniqueId, address.Id);
            return new CreateTenancyDetailsSubmissionOutcome
            {
                IsRejected = false,
                TenancyDetailsSubmissionUniqueId = tenancyDetailsSubmission.UniqueId
            };
        }

        public async Task<UserSubmissionSummary> GetUserSubmissionSummary(string userId)
        {
            var submissions = await _dbContext.TenancyDetailsSubmissions
                .Include(x => x.TenantVerifications)
                .Include(x => x.Address)
                .Include(x => x.Address.Country)
                .Where(x => x.UserId.Equals(userId))
                .ToListAsync();

            return new UserSubmissionSummary
            {
                tenancyDetailsSubmissions = submissions.Select(x => x.ToInfo()).ToList()
            };
        }

        private async Task<bool> TooManyRecentSubmissionsExist(long addressId)
        {
            var maxFrequency = _tenancyDetailsSubmissionServiceConfig.Create_MaxFrequencyPerAddress;

            var windowStart = _clock.OffsetNow - maxFrequency.Period;
            var actualTimes = await _dbContext.TenancyDetailsSubmissions
                .Where(s => s.AddressId.Equals(addressId))
                .Where(a => a.CreatedOn > windowStart)
                .CountAsync();

            return actualTimes >= maxFrequency.Times;
        }

        private async Task<TenancyDetailsSubmission> DoCreate(
            string userId,
            string userIpAddress,
            Guid submissionUniqueId,
            long addressId)
        {
            var entity = new TenancyDetailsSubmission
            {
                UniqueId = submissionUniqueId,
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
