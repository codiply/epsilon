using Epsilon.Logic.Entities;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.Services.Interfaces.OutgoingVerification;
using Epsilon.Logic.SqlContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Transactions;
using Epsilon.Resources.Logic.OutgoingVerification;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Resources.Common;
using Epsilon.Logic.Wrappers.Interfaces;
using Epsilon.Logic.Helpers;
using static Epsilon.Logic.Helpers.RandomStringHelper;
using Epsilon.Logic.Constants;

namespace Epsilon.Logic.Services
{
    public class OutgoingVerificationService : IOutgoingVerificationService
    {
        private readonly IClock _clock;
        private readonly IEpsilonContext _dbContext;
        private readonly IAntiAbuseService _antiAbuseService;
        private readonly IOutgoingVerificationServiceConfig _outgoingVerificationServiceConfig;
        private readonly IRandomFactory _randomFactory;

        public OutgoingVerificationService(
            IClock clock,
            IEpsilonContext dbContext,
            IAntiAbuseService antiAbuseService,
            IOutgoingVerificationServiceConfig outgoingVerificationServiceConfig,
            IRandomFactory randomFactory)
        {
            _clock = clock;
            _dbContext = dbContext;
            _antiAbuseService = antiAbuseService;
            _outgoingVerificationServiceConfig = outgoingVerificationServiceConfig;
            _randomFactory = randomFactory;
        }

        public async Task<bool> VerificationIsAssignedToUser(string userId, Guid verificationUniqueId)
        {
            // TODO_PANOS_TEST
            var verification = await _dbContext.TenantVerifications
                .SingleOrDefaultAsync(v => v.AssignedToId.Equals(userId) && v.UniqueId.Equals(verificationUniqueId));
            return verification != null;
        }

        public async Task<TenantVerification> GetVerificationForUser(string assignedUserId, Guid uniqueId)
        {
            var submission = await _dbContext.TenantVerifications
                .Include(x => x.TenancyDetailsSubmission)
                .Include(s => s.TenancyDetailsSubmission.Address)
                .Where(s => s.UniqueId.Equals(uniqueId))
                .Where(s => s.AssignedToId.Equals(assignedUserId))
                .SingleOrDefaultAsync();

            return submission;
        }

        public async Task<MyOutgoingVerificationsSummaryResponse> GetUserOutgoingVerificationsSummary(
            string userId, MyOutgoingVerificationsSummaryRequest request)
        {
            var query = _dbContext.TenantVerifications
                .Where(x => x.AssignedToId.Equals(userId))
                .OrderByDescending(x => x.CreatedOn);

            List<TenantVerification> verifications;
            var moreItemsExist = false;
            if (request.limitItemsReturned)
            {
                var limit = _outgoingVerificationServiceConfig.MyOutgoingVerificationsSummary_ItemsLimit;
                verifications = await query.Take(limit + 1).ToListAsync();
                if (verifications.Count > limit)
                {
                    moreItemsExist = true;
                    verifications = verifications.Take(limit).ToList();
                }
            }
            else
            {
                verifications = await query.ToListAsync();
            }

            return new MyOutgoingVerificationsSummaryResponse
            {
                moreItemsExist = moreItemsExist,
                tenantVerifications = verifications.Select(x => x.ToInfo()).ToList()
            };
        }

        public async Task<PickVerificationOutcome> Pick(
            string userId,
            string userIpAddress,
            Guid verificationUniqueId)
        {
            if (_outgoingVerificationServiceConfig.GlobalSwitch_DisablePickOutgoingVerification)
                return new PickVerificationOutcome
                {
                    IsRejected = true,
                    RejectionReason = OutgoingVerificationResources.GlobalSwitch_PickOutgoingVerificationDisabled_Message,
                    VerificationUniqueId = null
                };

            var antiAbuseServiceResponse = await _antiAbuseService.CanPickOutgoingVerification(userId, userIpAddress);
            if (antiAbuseServiceResponse.IsRejected)
                return new PickVerificationOutcome
                {
                    IsRejected = true,
                    RejectionReason = antiAbuseServiceResponse.RejectionReason,
                    VerificationUniqueId = null
                };

            // TODO_PANOS_TEST
            var verificationsPerTenancyDetailsSubmission = _outgoingVerificationServiceConfig.VerificationsPerTenancyDetailsSubmission;

            // TODO_PANOS_TEST
            var submissionIdsToAvoid = await _dbContext.TenantVerifications
                .Where(v => v.AssignedToId.Equals(userId) || v.AssignedByIpAddress.Equals(userId))
                .Select(v => v.TenancyDetailsSubmissionId)
                .Distinct()
                .ToListAsync();

            // TODO_PANOS_TEST
            // TODO_PANOS: pick a submission from the same country.
            var pickedSubmission = await _dbContext.TenancyDetailsSubmissions
                .Include(s => s.Address)
                .Include(s => s.TenantVerifications)
                .Where(s => s.UserId != userId)
                .Where(s => s.CreatedByIpAddress != userIpAddress)
                .Where(s => s.Address.CreatedById != userId)
                .Where(s => s.Address.CreatedByIpAddress != userIpAddress)
                .Where(s => s.TenantVerifications.Count() < verificationsPerTenancyDetailsSubmission)
                .Where(s => !submissionIdsToAvoid.Contains(s.Id))
                .OrderBy(s => s.TenantVerifications.Count())
                .FirstOrDefaultAsync();

            if (pickedSubmission == null)
            {
                // TODO_PANOS_TEST
                return new PickVerificationOutcome
                {
                    IsRejected = true,
                    RejectionReason = OutgoingVerificationResources.Pick_NoVerificationAssignableToUser_RejectionMessage,
                    VerificationUniqueId = null
                };
            }

            var random = _randomFactory.Create(_clock.OffsetNow.Millisecond);

            var tenantVerification = new TenantVerification()
            {
                UniqueId = verificationUniqueId,
                TenancyDetailsSubmissionId = pickedSubmission.Id,
                AssignedToId = userId,
                AssignedByIpAddress = userIpAddress,
                SecretCode = RandomStringHelper.GetString(random, AppConstant.SECRET_CODE_MAX_LENGTH, CharacterCase.Upper)
            };

            _dbContext.TenantVerifications.Add(tenantVerification);
            await _dbContext.SaveChangesAsync();

            // TODO_PANOS_TEST
            return new PickVerificationOutcome
            {
                IsRejected = false,
                VerificationUniqueId = tenantVerification.UniqueId
            };
        }

        public async Task<MarkVerificationAsSentOutcome> MarkAsSent(
            string userId,
            Guid verificationUniqueId)
        {
            var verification = await GetVerificationForUser(userId, verificationUniqueId);
            if (verification == null)
            {
                // TODO_PANOS_TEST
                return new MarkVerificationAsSentOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidRequestMessage
                };
            }

            if (!verification.CanMarkAsSent())
            {
                // TODO_PANOS_TEST
                return new MarkVerificationAsSentOutcome
                {
                    IsRejected = true,
                    RejectionReason = CommonResources.GenericInvalidActionMessage
                };
            }

            verification.MarkedAsSentOn = _clock.OffsetNow;
            _dbContext.Entry(verification).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            // TODO_PANOS_TEST
            return new MarkVerificationAsSentOutcome
            {
                IsRejected = false
            };
        }
    }
}
