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
using Epsilon.Logic.Services.Interfaces.TenancyDetailsSubmission;
using Epsilon.Logic.Forms.Submission;
using Epsilon.Resources.Common;

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

        public async Task<MySubmissionsSummaryResponse> GetUserSubmissionsSummary(string userId, MySubmissionsSummaryRequest request)
        {
            var query = _dbContext.TenancyDetailsSubmissions
                .Include(x => x.TenantVerifications)
                .Include(x => x.Address)
                .Include(x => x.Address.Country)
                .Where(x => x.UserId.Equals(userId))
                .OrderByDescending(x => x.CreatedOn);

            List<TenancyDetailsSubmission> submissions;
            var moreItemsExist = false;
            if (request.limitItemsReturned)
            {
                var limit = _tenancyDetailsSubmissionServiceConfig.MySubmissionsSummary_ItemsLimit;
                submissions = await query.Take(limit + 1).ToListAsync();
                if (submissions.Count > limit)
                {
                    moreItemsExist = true;
                    submissions = submissions.Take(limit).ToList();
                }
            }
            else
            {
                submissions = await query.ToListAsync();
            }        

            return new MySubmissionsSummaryResponse
            {
                moreItemsExist = moreItemsExist,
                tenancyDetailsSubmissions = submissions.Select(x => x.ToInfo()).ToList()
            };
        }

        public async Task<CreateTenancyDetailsSubmissionOutcome> Create(
            string userId, 
            string userIpAddress, 
            Guid submissionUniqueId,
            Guid addressUniqueId)
        {
            if (_tenancyDetailsSubmissionServiceConfig.GlobalSwitch_DisableCreateTenancyDetailsSubmission)
                return new CreateTenancyDetailsSubmissionOutcome
                {
                    IsRejected = true,
                    RejectionReason = TenancyDetailsSubmissionResources.GlobalSwitch_CreateTenancyDetailsSubmissionDisabled_Message,
                    ReturnToForm = false,
                    TenancyDetailsSubmissionUniqueId = null
                };

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

        public async Task<EnterVerificationCodeOutcome> EnterVerificationCode(string userId, VerificationCodeForm form)
        {
            var submission = await GetSubmissionForUser(userId, form.TenancyDetailsSubmissionUniqueId);
            if (submission == null || !submission.CanEnterVerificationCode())
            {
                return new EnterVerificationCodeOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidRequestMessage
                };
            }

            // TODO_PANOS
            throw new NotImplementedException();
        }

        public async Task<SubmitTenancyDetailsOutcome> SubmitTenancyDetails(string userId, TenancyDetailsForm form)
        {
            var submission = await GetSubmissionForUser(userId, form.TenancyDetailsSubmissionUniqueId);
            if (submission == null || !submission.CanSubmitTenancyDetails())
            {
                return new SubmitTenancyDetailsOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidRequestMessage
                };
            }

            // TODO_PANOS
            throw new NotImplementedException();
        }

        public async Task<SubmitMoveOutDetailsOutcome> SubmitMoveOutDetails(string userId, MoveOutDetailsForm form)
        {
            var submission = await GetSubmissionForUser(userId, form.TenancyDetailsSubmissionUniqueId);
            if (submission == null || submission.CanSubmitMoveOutDetails())
            {
                return new SubmitMoveOutDetailsOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidRequestMessage
                };
            }

            // TODO_PANOS
            throw new NotImplementedException();
        }

        private async Task<TenancyDetailsSubmission> GetSubmissionForUser(string userId, Guid uniqueId)
        {
            var submission = await  _dbContext.TenancyDetailsSubmissions
                .Include(s => s.Address)
                .Include(s => s.TenantVerifications)
                .Where(s => s.UniqueId.Equals(uniqueId))
                .Where(s => s.UserId.Equals(userId))
                .SingleOrDefaultAsync();

            return submission;
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
