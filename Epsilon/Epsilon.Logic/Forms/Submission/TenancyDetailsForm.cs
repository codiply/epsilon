using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.ResourceNames.Web;
using Epsilon.Logic.Entities;
using Epsilon.Resources.Web.Submission;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Epsilon.Logic.Forms.Submission
{
    [Bind(Exclude = "DisplayAddress")]
    public class TenancyDetailsForm
    {
        public string DisplayAddress { get; set; }

        public Guid TenancyDetailsSubmissionUniqueId { get; set; }

        public string CurrencySybmol { get; set; }

        [Required(ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_FieldRentPerMonth_RequiredErrorMessage)]
        [Range(0.0, double.MaxValue,
            ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_FieldRentPerMonth_RangeErrorMessage)]
        [Display(ResourceType = typeof(SubmissionResources), Name = SubmissionResourceName.SubmitTenancyDetailsForm_FieldRentPerMonth_DisplayName)]
        public decimal RentPerMonth { get; set; }

        [Required(ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_FieldNumberOfBedrooms_RequiredErrorMessage)]
        [Range(0, AppConstant.MAX_NUMBERS_OF_BEDROOMS,
            ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_FieldNumberOfBedrooms_RangeErrorMessage)]
        [Display(ResourceType = typeof(SubmissionResources), Name = SubmissionResourceName.SubmitTenancyDetailsForm_FieldNumberOfBedrooms_DisplayName)]
        public byte NumberOfBedrooms { get; set; }

        [Required(ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_FieldIsPartOfProperty_RequiredErrorMessage)]
        [Display(ResourceType = typeof(SubmissionResources), Name = SubmissionResourceName.SubmitTenancyDetailsForm_FieldIsPartOfProperty_DisplayName)]
        public bool IsPartOfProperty { get; set; }

        [Required(ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_FieldIsFurnished_RequiredErrorMessage)]
        [Display(ResourceType = typeof(SubmissionResources), Name = SubmissionResourceName.SubmitTenancyDetailsForm_FieldIsFurnished_DisplayName)]
        public bool IsFurnished { get; set; }

        [Required(ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_FieldPropertyConditionRating_RequiredErrorMessage)]
        [Range(AppConstant.RATING_MIN_VALUE, AppConstant.RATING_MAX_VALUE,
            ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_RatingField_RangeErrorMessage)]
        [Display(ResourceType = typeof(SubmissionResources), Name = SubmissionResourceName.SubmitTenancyDetailsForm_FieldPropertyConditionRating_DisplayName)]
        public byte PropertyConditionRating { get; set; }

        [Required(ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_FieldLandlordRating_RequiredErrorMessage)]
        [Range(AppConstant.RATING_MIN_VALUE, AppConstant.RATING_MAX_VALUE,
            ErrorMessageResourceType = typeof(SubmissionResources),
            ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_RatingField_RangeErrorMessage)]
        [Display(ResourceType = typeof(SubmissionResources), Name = SubmissionResourceName.SubmitTenancyDetailsForm_FieldLandlordRating_DisplayName)]
        public byte LandlordRating { get; set; }

        [Required(ErrorMessageResourceType = typeof(SubmissionResources),
                    ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_FieldNeighboursRating_RequiredErrorMessage)]
        [Range(AppConstant.RATING_MIN_VALUE, AppConstant.RATING_MAX_VALUE,
                    ErrorMessageResourceType = typeof(SubmissionResources),
                    ErrorMessageResourceName = SubmissionResourceName.SubmitTenancyDetailsForm_RatingField_RangeErrorMessage)]
        [Display(ResourceType = typeof(SubmissionResources), Name = SubmissionResourceName.SubmitTenancyDetailsForm_FieldNeighboursRating_DisplayName)]
        public byte NeighboursRating { get; set; }

        public bool ReturnToSummary { get; set; }

        public void ApplyOnEntity(TenancyDetailsSubmission entity)
        {
            entity.RentPerMonth = RentPerMonth;
            entity.NumberOfBedrooms = NumberOfBedrooms;
            entity.IsPartOfProperty = IsPartOfProperty;
            entity.IsFurnished = IsFurnished;
            entity.PropertyConditionRating = PropertyConditionRating;
            entity.LandlordRating = LandlordRating;
            entity.NeighboursRating = NeighboursRating;
        }
    }
}
