using Epsilon.Logic.Constants.ResourceNames.Web;
using Epsilon.Resources.Web.Submission;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Forms.Submission
{
    public class MoveOutDetailsForm
    {
        public Guid TenancyDetailsSubmissionUniqueId { get; set; }

        [Required(ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.MoveOutDetailsForm_FieldMoveOutDate_RequiredErrorMessage)]
        [Display(ResourceType = typeof(SubmissionResources), Name = SubmissionResourceName.MoveOutDetailsForm_FieldMoveOutDate_DisplayName)]
        public DateTime? MoveOutDate { get; set; }

        public bool ReturnToSummary { get; set; }
    }
}
