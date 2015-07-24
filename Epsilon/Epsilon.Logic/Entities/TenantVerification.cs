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
        public virtual string Code { get; set; }
        public virtual DateTimeOffset CreatedOn { get; set; }
        public virtual DateTimeOffset? SentOn { get; set; }
        public virtual DateTimeOffset? VerifiedOn { get; set; }
        public virtual string CreatedByIpAddress { get; set; }

        [Timestamp]
        public virtual Byte[] Timestamp { get; set; }

        public virtual TenancyDetailsSubmission TenancyDetailsSubmission { get; set; }
    }
}
