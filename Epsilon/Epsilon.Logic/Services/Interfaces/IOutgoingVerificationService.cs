using Epsilon.Logic.Entities;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Services.Interfaces.OutgoingVerification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    namespace OutgoingVerification
    {
        public class BaseOutcome
        {
            public bool IsRejected { get; set; }
            public string RejectionReason { get; set; }
        }

        public class PickVerificationOutcome : BaseOutcome
        {
            public Guid? VerificationUniqueId { get; set; }
        }

        public class MarkVerificationAsSentOutcome : BaseOutcome
        {
        }
    }

    public interface IOutgoingVerificationService
    {
        Task<TenantVerification> GetVerificationForUser(string assingedUserId, Guid uniqueId);

        Task<MyOutgoingVerificationsSummaryResponse> GetUserOutgoingVerificationsSummary(
            string userId, MyOutgoingVerificationsSummaryRequest request);

        Task<PickVerificationOutcome> Pick(
            string userId,
            string userIpAddress,
            Guid verificationUniqueId);

        Task<MarkVerificationAsSentOutcome> MarkAsSent(
            string userId,
            Guid verificationUniqueId);
    }
}
