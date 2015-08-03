using Epsilon.Logic.Dtos;
using Epsilon.Logic.Entities;
using Epsilon.Logic.Forms.Submission;
using Epsilon.Logic.JsonModels;
using Epsilon.Logic.Services.Interfaces.TenancyDetailsSubmission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    namespace TenancyDetailsSubmission
    {
        public class BaseOutcome
        {
            public bool IsRejected { get; set; }
            public string RejectionReason { get; set; }
            public bool ReturnToForm { get; set; }
            public IList<UiAlert> UiAlerts { get; set; }
        }

        public class CreateTenancyDetailsSubmissionOutcome : BaseOutcome
        {
            public Guid? TenancyDetailsSubmissionUniqueId { get; set; }
        }

        public class EnterVerificationCodeOutcome : BaseOutcome
        {
        }

        public class SubmitTenancyDetailsOutcome : BaseOutcome
        {
        }

        public class SubmitMoveOutDetailsOutcome : BaseOutcome
        {
        }

        public class GetSubmissionCountryOutcome
        {
            public bool SubmissionNotFound { get; set; }
            public Country Country { get; set; }
        }
    }
    
    public interface ITenancyDetailsSubmissionService
    {
        Task<MySubmissionsSummaryResponse> GetUserSubmissionsSummaryWithCaching(string userId, bool limitItemsReturned);

        Task<MySubmissionsSummaryResponse> GetUserSubmissionsSummary(string userId, bool limitItemsReturned);

        Task<CreateTenancyDetailsSubmissionOutcome> Create(
            string userId,
            string userIpAddress,
            Guid submissionUniqueId,
            Guid addressUniqueId);

        Task<EnterVerificationCodeOutcome> EnterVerificationCode(string userId, VerificationCodeForm form);

        Task<SubmitTenancyDetailsOutcome> SubmitTenancyDetails(string userId, TenancyDetailsForm form);

        Task<SubmitMoveOutDetailsOutcome> SubmitMoveOutDetails(string userId, MoveOutDetailsForm form);

        Task<bool> SubmissionBelongsToUser(string userId, Guid submissionUniqueId);

        Task<GetSubmissionCountryOutcome> GetSubmissionCountry(string userId, Guid submissionUniqueId);
    }
}
