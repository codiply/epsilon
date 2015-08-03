using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.ResourceNames.Web;
using Epsilon.Resources.Web.Submission;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Forms.Submission
{
    public class TenancyDetailsForm
    {
        public Guid TenancyDetailsSubmissionUniqueId { get; set; }

        public string CurrencySybmol { get; set; }

        [Required(ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_FieldRent_RequiredErrorMessage)]
        [Range(0.0, double.MaxValue,
            ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_FieldRent_RangeErrorMessage)]
        [Display(ResourceType = typeof(SubmissionResources), Name = SubmissionResourceName.SubmitTenancyDetailsForm_FieldRent_DisplayName)]
        public decimal Rent { get; set; }

        [Required(ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_FieldNumberOfBedrooms_RequiredErrorMessage)]
        [Range(0, AppConstant.MAX_NUMBERS_OF_BEDROOMS,
            ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_FieldNumberOfBedrooms_RangeErrorMessage)]
        [Display(ResourceType = typeof(SubmissionResources), Name = SubmissionResourceName.SubmitTenancyDetailsForm_FieldNumberOfBedrooms_DisplayName)]
        public int? NumberOfBedrooms { get; set; }

        [Required(ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_FieldIsPartOfProperty_RequiredErrorMessage)]
        [Display(ResourceType = typeof(SubmissionResources), Name = SubmissionResourceName.SubmitTenancyDetailsForm_FieldIsPartOfProperty_DisplayName)]
        public bool IsPartOfProperty { get; set; }

        [Required(ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_FieldMoveInDate_RequiredErrorMessage)]
        [Display(ResourceType = typeof(SubmissionResources), Name = SubmissionResourceName.SubmitTenancyDetailsForm_FieldMoveInDate_DisplayName)]
        public DateTime? MoveInDate { get; set; }
        
        public bool ReturnToSummary { get; set; }
    }
}
