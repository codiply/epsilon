using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Forms.Submission
{
    public class MoveOutDetailsForm
    {
        public Guid TenancyDetailsSubmissionUniqueId { get; set; }
        public DateTime? MoveOutDate { get; set; }
        public bool ReturnToSummary { get; set; }
    }
}
