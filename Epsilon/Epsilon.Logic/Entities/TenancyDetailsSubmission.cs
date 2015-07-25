using Epsilon.Logic.JsonModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class TenancyDetailsSubmission
    {
        public virtual long Id { get; set; }
        public virtual Guid UniqueId { get; set; }
        public virtual string UserId { get; set; }
        public virtual long AddressId { get; set; }
        public virtual Decimal? Rent { get; set; }
        public virtual string CurrencyId { get; set; }
        public virtual int? NumberOfBedrooms { get; set; }
        public virtual bool? IsPartOfProperty { get; set; }
        public virtual DateTime? MoveInDate { get; set; }
        public virtual DateTime? MoveOutDate { get; set; }
        public virtual DateTimeOffset CreatedOn { get; set; }
        public virtual DateTimeOffset? SubmittedOn { get; set; }
        public virtual string CreatedByIpAddress { get; set; }

        [Timestamp]
        public virtual Byte[] Timestamp { get; set; }

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
            return TenantVerifications.Any(v => v.SentOn.HasValue);
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
            return SubmittedOn.HasValue && Rent.HasValue;
        }

        public bool StepMoveOutDetailsSubmittedDone()
        {
            return MoveOutDate.HasValue;
        }

        /// <summary>
        /// Note: You will need to Include the TenantVerifications in your entity for this to work.
        /// </summary>
        /// <returns></returns>
        public bool CanEnterVerificationCode()
        {
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
        
        public bool CanEnterMoveOutDetails()
        {
            return StepTenancyDetailsSubmittedDone() && !StepMoveOutDetailsSubmittedDone();
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
                canEnterMoveOutDetails = CanEnterMoveOutDetails(),
                stepVerificationCodeSentOutDone = StepVerificationCodeSentOutDone(),
                stepVerificationCodeEnteredDone = StepVerificationCodeEnteredDone(),
                stepTenancyDetailsSubmittedDone = StepTenancyDetailsSubmittedDone(),
                stepMoveOutDetailsSubmittedDone = StepMoveOutDetailsSubmittedDone()
            };
        }
    }
}
