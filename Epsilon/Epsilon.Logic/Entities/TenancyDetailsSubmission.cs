using Epsilon.Logic.JsonModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Epsilon.Logic.Entities
{
    public class TenancyDetailsSubmission
    {
        public virtual long Id { get; set; }
        public virtual Guid UniqueId { get; set; }
        public virtual string UserId { get; set; }
        public virtual long AddressId { get; set; }
        public virtual decimal? RentPerMonth { get; set; }
        public virtual string CurrencyId { get; set; }
        public virtual byte? NumberOfBedrooms { get; set; }
        public virtual bool? IsPartOfProperty { get; set; }
        public virtual bool? IsFurnished { get; set; }
        public virtual byte? LandlordRating { get; set; }
        public virtual byte? PropertyConditionRating { get; set; }
        public virtual byte? NeighboursRating { get; set; }
        public virtual DateTimeOffset CreatedOn { get; set; }
        public virtual DateTimeOffset? SubmittedOn { get; set; }
        public virtual string CreatedByIpAddress { get; set; }

        // This is for later convenience in order to hide some submissions from property information if needed.
        public virtual bool IsHidden { get; set; }

        [Timestamp]
        public virtual byte[] Timestamp { get; set; }

        public virtual User User { get; set; }
        public virtual Address Address { get; set; }
        public virtual Currency Currency { get; set; }
        public virtual ICollection<TenantVerification> TenantVerifications { get; set; }

        /// <summary>
        /// Note: You will need to Include the TenantVerifications in your entity for this to work.
        /// </summary>
        /// <returns></returns>
        public bool StepVerificationCodeSentOutDone()
        {
            return TenantVerifications.Any(v => v.MarkedAsSentOn.HasValue);
        }

        /// <summary>
        /// Note: You will need to Include the TenantVerifications in your entity for this to work.
        /// </summary>
        /// <returns></returns>
        public bool StepVerificationCodeEnteredDone()
        {
            return TenantVerifications.Any(v => v.VerifiedOn.HasValue);
        }

        public bool StepTenancyDetailsSubmittedDone()
        {
            return SubmittedOn.HasValue && RentPerMonth.HasValue;
        }

        /// <summary>
        /// Note: You will need to Include the TenantVerifications in your entity for this to work.
        /// </summary>
        /// <returns></returns>
        public bool CanEnterVerificationCode()
        {
            // I do not mind if the verifications have value for MarkedAsSentOn. 
            // It is possible that the assigned user forgot to mark them as sent.
            return TenantVerifications.Any(v => !v.VerifiedOn.HasValue);
        }

        /// <summary>
        /// Note: You will need to Include the TenantVerifications in your entity for this to work.
        /// </summary>
        /// <returns></returns>
        public bool CanSubmitTenancyDetails()
        {
            return StepVerificationCodeEnteredDone() && !StepTenancyDetailsSubmittedDone();
        }

        /// <summary>
        /// Note: You will need to Include Address, Address.Country and 
        ///       TenantVerifications in your entity for this to work.
        /// </summary>
        /// <returns></returns>
        public TenancyDetailsSubmissionInfo ToInfo()
        {
            return new TenancyDetailsSubmissionInfo
            {
                uniqueId = UniqueId,
                displayAddress = this.Address.FullAddress(),
                canEnterVerificationCode = CanEnterVerificationCode(),
                canSubmitTenancyDetails = CanSubmitTenancyDetails(),
                stepVerificationCodeSentOutDone = StepVerificationCodeSentOutDone(),
                stepVerificationCodeEnteredDone = StepVerificationCodeEnteredDone(),
                stepTenancyDetailsSubmittedDone = StepTenancyDetailsSubmittedDone()
            };
        }
    }
}
