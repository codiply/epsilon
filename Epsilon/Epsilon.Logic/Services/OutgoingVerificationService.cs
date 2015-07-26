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

namespace Epsilon.Logic.Services
{
    public class OutgoingVerificationService : IOutgoingVerificationService
    {
        private readonly IEpsilonContext _dbContext;
        private readonly IAntiAbuseService _antiAbuseService;

        public OutgoingVerificationService(
            IEpsilonContext dbContext,
            IAntiAbuseService antiAbuseService)
        {
            _dbContext = dbContext;
            _antiAbuseService = antiAbuseService;
        }

        public async Task<PickVerificationOutcome> Pick(
            string userId,
            string userIpAddress,
            Guid verificationUniqueId)
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var antiAbuseServiceResponse = await _antiAbuseService.CanPickOutgoingVerifications(userId, userIpAddress);
                if (antiAbuseServiceResponse.IsRejected)
                    return new PickVerificationOutcome
                    {
                        IsRejected = true,
                        RejectionReason = antiAbuseServiceResponse.RejectionReason,
                        VerificationUniqueId = null
                    };

                // TODO_PANOS: put this in the config
                var maxVerifications = 2;

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
                    .Where(s => s.TenantVerifications.Count() < maxVerifications)
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
                    SecretCode = "secret-code" // PANOS_TODO
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
