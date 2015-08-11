using Epsilon.Logic.Constants.ResourceNames.Web;
using Epsilon.Resources.Web.Submission;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Epsilon.Logic.Forms.Submission
{
    public class VerificationCodeForm
    {
        public string DisplayAddress { get; set; }

        public Guid TenancyDetailsSubmissionUniqueId { get; set; }

        [Required(ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.VerificationCodeForm_FieldVerificationCode_RequiredErrorMessage)]
        [Display(ResourceType = typeof(SubmissionResources), Name = SubmissionResourceName.VerificationCodeForm_FieldVerificationCode_DisplayName)]
        public string VerificationCode { get; set; }

        public bool ReturnToSummary { get; set; }
    }
}
