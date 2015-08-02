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

        public Decimal Rent { get; set; }
        public int? NumberOfBedrooms { get; set; }
        public bool? IsPartOfProperty { get; set; }
        public DateTime? MoveInDate { get; set; }

        public bool ReturnToSummary { get; set; }
    }
}
