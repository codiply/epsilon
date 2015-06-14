using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class TenancyDetailsSubmission : BaseEntity
    {
        public virtual Guid Id { get; set; }
        public virtual string UserId { get; set; }
        public virtual string AddressId { get; set; }
        public virtual Decimal Rent { get; set; }
        public virtual int? BedroomNumber { get; set; }
        public virtual bool? RentingPartOfProperty { get; set; }
        public virtual DateTimeOffset CreatedOn { get; set; }
        public virtual DateTimeOffset? SubmittedOn { get; set; }

        public virtual User User { get; set; }
        public virtual Address Address { get; set; }
        public virtual ICollection<TenantVerification> TenantVerifications { get; set; }
    }
}
