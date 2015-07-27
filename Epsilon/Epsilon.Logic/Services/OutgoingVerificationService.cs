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

namespace Epsilon.Logic.Services
{
    public class OutgoingVerificationService : IOutgoingVerificationService
    {
        private readonly IEpsilonContext _dbContext;
        private readonly IAntiAbuseService _antiAbuseService;
        private readonly IOutgoingVerificationServiceConfig _outgoingVerificationServiceConfig;

        public OutgoingVerificationService(
            IEpsilonContext dbContext,
            IAntiAbuseService antiAbuseService,
            IOutgoingVerificationServiceConfig outgoingVerificationServiceConfig)
        {
            _dbContext = dbContext;
            _antiAbuseService = antiAbuseService;
            _outgoingVerificationServiceConfig = outgoingVerificationServiceConfig;
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
                var limit = 2; // TODO_PANOS: put this in a config
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
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // TODO_PANOS_TEST
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

                // TODO_PANOS: put this in the config
                var maxVerificationsPerSubmission = 2;

                var submissionIdsToAvoid = await _dbContext.TenantVerifications
                    .Where(v => v.AssignedToId.Equals(userId) || v.AssignedByIpAddress.Equals(userId))
                    .Select(v => v.TenancyDetailsSubmissionId)
                    .Distinct()
                    .ToListAsync();

                // TODO_PANOS: pick a submission from the same country.
                var pickedSubmission = await _dbContext.TenancyDetailsSubmissions
                    .Include(s => s.Address)
                    .Include(s => s.TenantVerifications)
                    .Where(s => s.UserId != userId)
                    .Where(s => s.CreatedByIpAddress != userIpAddress)
                    .Where(s => s.Address.CreatedById != userId)
                    .Where(s => s.Address.CreatedByIpAddress != userIpAddress)
                    .Where(s => s.TenantVerifications.Count() < maxVerificationsPerSubmission)
                    .Where(s => !submissionIdsToAvoid.Contains(s.Id))
                    .OrderBy(s => s.TenantVerifications.Count())
                    .FirstOrDefaultAsync();
                
                if (pickedSubmission == null)
                {
                    return new PickVerificationOutcome
                    {
                        IsRejected = true,
                        RejectionReason = OutgoingVerificationResources.Pick_NoVerificationAssignableToUser_RejectionMessage,
                        VerificationUniqueId = null
                    };
                }

                var tenantVerification = new TenantVerification()
                {
                    UniqueId = verificationUniqueId,
                    TenancyDetailsSubmissionId = pickedSubmission.Id,
                    AssignedToId = userId,
                    AssignedByIpAddress = userIpAddress,
                    SecretCode = "secret-code" // TODO_PANOS
                };

                _dbContext.TenantVerifications.Add(tenantVerification);
                await _dbContext.SaveChangesAsync();

                transaction.Complete();

                return new PickVerificationOutcome
                {
                    IsRejected = false,
                    VerificationUniqueId = tenantVerification.UniqueId
                };
            }
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

        public async Task<MarkVerificationAsSentOutcome> MarkAsSent(
            string userId,
            Guid verificationUniqueId)
        {
            var verification = await GetVerificationForUser(userId, verificationUniqueId);
            if (verification == null || !verification.CanMarkAsSent())
            {
                return new MarkVerificationAsSentOutcome
                {
                    IsRejected = true,
                    // TODO_PANOS: put in a resource, use the same resource accross all 3 methods
                    RejectionReason = "Sorry something went wrong."
                };
            }

            // TODO_PANOS
            throw new NotImplementedException();
        }
    }
}
