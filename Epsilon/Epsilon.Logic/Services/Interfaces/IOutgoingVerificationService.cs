using Epsilon.Logic.Models;
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

        public class BaseOutcomeWithAlerts : BaseOutcome
        {
            public IList<UiAlert> UiAlerts { get; set; }
        }

        public class PickVerificationOutcome : BaseOutcomeWithAlerts
        {
            public Guid? VerificationUniqueId { get; set; }
        }

        public class MarkVerificationAsSentOutcome : BaseOutcomeWithAlerts
        {
        }

        public class MarkAddressAsInvalidOutcome : BaseOutcomeWithAlerts
        {
        }

        public class GetInstructionsOutcome : BaseOutcome
        {
            public OutgoingVerificationInstructionsModel Instructions { get; set; }
        }

        public class GetVerificationMessageOutcome : BaseOutcome
        {
            public VerificationMessageArgumentsModel MessageArguments { get; set; }
        }
    }

    public interface IOutgoingVerificationService
    {
        Task<MyOutgoingVerificationsSummaryResponse> GetUserOutgoingVerificationsSummaryWithCaching(
            string userId, bool limitItemsReturned);

        Task<MyOutgoingVerificationsSummaryResponse> GetUserOutgoingVerificationsSummary(
            string userId, bool limitItemsReturned);

        Task<PickVerificationOutcome> Pick(
            string userId,
            string userIpAddress,
            Guid verificationUniqueId);

        Task<GetInstructionsOutcome> GetInstructions(string userId, Guid verificationUniqueId);

        Task<GetVerificationMessageOutcome> GetVerificationMessage(string userId, Guid verificationUniqueId);

        Task<MarkVerificationAsSentOutcome> MarkAsSent(string userId, Guid verificationUniqueId);

        Task<MarkAddressAsInvalidOutcome> MarkAddressAsInvalid(string userId, Guid verificationUniqueId);

        Task<bool> VerificationIsAssignedToUser(string userId, Guid verificationUniqueId);
    }
}
