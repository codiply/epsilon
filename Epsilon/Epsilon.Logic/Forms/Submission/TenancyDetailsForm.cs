using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Forms.Submission
{
    public class TenancyDetailsForm
    {
        public Guid TenancyDetailsSubmissionUniqueId { get; set; }

        public virtual Decimal Rent { get; set; }
        public virtual int? NumberOfBedrooms { get; set; }
        public virtual bool? IsPartOfProperty { get; set; }
        public virtual DateTime? MoveInDate { get; set; }
    }
}
