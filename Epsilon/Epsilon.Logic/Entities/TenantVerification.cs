using Epsilon.Logic.JsonModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class TenantVerification
    {
        public virtual long Id { get; set; }
        public virtual Guid UniqueId { get; set; }
        public virtual long TenancyDetailsSubmissionId { get; set; }
        public virtual string SecretCode { get; set; }
        public virtual DateTimeOffset CreatedOn { get; set; }
        public virtual DateTimeOffset? SentOn { get; set; }
        public virtual DateTimeOffset? VerifiedOn { get; set; }
        public virtual string AssignedToId { get; set; }
        public virtual string AssignedByIpAddress { get; set; }

        [Timestamp]
        public virtual Byte[] Timestamp { get; set; }

        public virtual User AssignedTo { get; set; }
        public virtual TenancyDetailsSubmission TenancyDetailsSubmission { get; set; }

        public bool StepVerificationSentOutDone()
        {
            return SentOn.HasValue;
        }

        public bool StepVerificationReceivedDone()
        {
            return VerifiedOn.HasValue;
        }

        public bool CanMarkAsSent()
        {
            return !StepVerificationSentOutDone();
        }

        public string DisplayId()
        {
            return UniqueId.ToString().ToUpperInvariant();
        }

        public TenantVerificationInfo ToInfo()
        {
            return new TenantVerificationInfo
            {
                uniqueId = UniqueId,
                displayId = DisplayId(),
                canMarkAsSent = CanMarkAsSent(),
                stepVerificationSentOutDone = StepVerificationSentOutDone(),
                stepVerificationReceivedDone = StepVerificationReceivedDone()
            };
        }
    }
}
