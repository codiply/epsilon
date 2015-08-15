using Epsilon.Logic.JsonModels;
using System;
using System.ComponentModel.DataAnnotations;

namespace Epsilon.Logic.Entities
{
    public class TenantVerification
    {
        public virtual long Id { get; set; }
        public virtual Guid UniqueId { get; set; }
        public virtual long TenancyDetailsSubmissionId { get; set; }
        public virtual string SecretCode { get; set; }
        public virtual DateTimeOffset CreatedOn { get; set; }
        public virtual DateTimeOffset? MarkedAsSentOn { get; set; }
        public virtual DateTimeOffset? VerifiedOn { get; set; }
        public virtual DateTimeOffset? MarkedAddressAsInvalidOn { get; set; }
        public virtual DateTimeOffset? SenderRewardedOn { get; set; }
        public virtual string AssignedToId { get; set; }
        public virtual string AssignedByIpAddress { get; set; }

        [Timestamp]
        public virtual Byte[] Timestamp { get; set; }

        public virtual User AssignedTo { get; set; }
        public virtual TenancyDetailsSubmission TenancyDetailsSubmission { get; set; }

        public bool MarkedAddressAsInvalid()
        {
            // TODO_TEST_PANOS
            return MarkedAddressAsInvalidOn.HasValue;
        }

        public bool StepVerificationSentOutDone()
        {
            return MarkedAsSentOn.HasValue;
        }

        public bool StepVerificationReceivedDone()
        {
            return VerifiedOn.HasValue;
        }

        public bool CanMarkAsSent()
        {
            return !StepVerificationSentOutDone() && !MarkedAddressAsInvalid();
        }

        public bool CanMarkAddressAsInvalid()
        {
            return !MarkedAddressAsInvalid() && !StepVerificationSentOutDone();
        }

        public bool CanViewInstructions(DateTimeOffset now, TimeSpan expiryPeriod)
        {
            // TODO_TEST_PANOS
            return !MarkedAddressAsInvalid() && now < ExpiresOn(expiryPeriod);
        }

        public bool IsSenderRewarded()
        {
            return SenderRewardedOn.HasValue;
        }

        public DateTimeOffset ExpiresOn(TimeSpan expiryPeriod)
        {
            // TODO_TEST_PANOS
            return CreatedOn + expiryPeriod;
        }

        /// <summary>
        /// Note: You will need to Include TenancyDetailsSubmission.Address for this to work.
        /// </summary>
        /// <returns></returns>
        public TenantVerificationInfo ToInfo(DateTimeOffset now, TimeSpan expiryPeriod)
        {
            return new TenantVerificationInfo
            {
                uniqueId = UniqueId,
                addressArea = TenancyDetailsSubmission.Address.LocalityRegionPostcode(),
                markedAddrressInvalid = MarkedAddressAsInvalid(),
                canMarkAsSent = CanMarkAsSent(),
                canViewInstructions = CanViewInstructions(now, expiryPeriod),
                stepVerificationSentOutDone = StepVerificationSentOutDone(),
                stepVerificationReceivedDone = StepVerificationReceivedDone()
            };
        }
    }
}
