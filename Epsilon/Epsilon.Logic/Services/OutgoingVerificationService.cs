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

namespace Epsilon.Logic.Services
{
    public class OutgoingVerificationService : IOutgoingVerificationService
    {
        private readonly IEpsilonContext _dbContext;

        public OutgoingVerificationService(
            IEpsilonContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PickVerificationOutcome> Pick(
            string userId,
            string userIpAddress,
            Guid verificationUniqueId)
        {
            // TODO_PANOS: Anti-abuse check

            // TODO_PANOS: make sure you don't pick a submission from the same user or ipaddress.


            throw new NotImplementedException();
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
