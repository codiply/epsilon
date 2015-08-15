using Epsilon.Logic.Models;
using Epsilon.Resources.Common;
using Epsilon.Resources.Web.Manage;
using System.ComponentModel.DataAnnotations;

namespace Epsilon.Web.Models
{
    public class IndexViewModel
    { 
        public bool HasPassword { get; set; }
        public UserPreferenceModel UserPreference { get; set; }
        //public IList<UserLoginInfo> Logins { get; set; }
        //public string PhoneNumber { get; set; }
        //public bool TwoFactor { get; set; }
        //public bool BrowserRemembered { get; set; }
    }

    //public class ManageLoginsViewModel
    //{
    //    public IList<UserLoginInfo> CurrentLogins { get; set; }
    //    public IList<AuthenticationDescription> OtherLogins { get; set; }
    //}

    //public class FactorViewModel
    //{
    //    public string Purpose { get; set; }
    //}

    //public class SetPasswordViewModel
    //{
    //    [Required]
    //    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    //    [DataType(DataType.Password)]
    //    [Display(Name = "New password")]
    //    public string NewPassword { get; set; }

    //    [DataType(DataType.Password)]
    //    [Display(Name = "Confirm new password")]
    //    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    //    public string ConfirmPassword { get; set; }
    //}

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ManageResources), ErrorMessageResourceName = "ChangePassword_FieldOldPassword_RequiredErrorMessage")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(ManageResources), Name = "ChangePassword_FieldOldPassword_DisplayName")]
        public string OldPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(ManageResources), ErrorMessageResourceName = "ChangePassword_FieldNewPassword_RequiredErrorMessage")]
        [StringLength(100, 
            ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "StringLengthErrorMessage", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(ManageResources), Name = "ChangePassword_FieldNewPassword_DisplayName")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(ManageResources), Name = "ChangePassword_FieldConfirmPassword_DisplayName")]
        [Compare("NewPassword", ErrorMessageResourceType = typeof(ManageResources), ErrorMessageResourceName = "ChangePassword_FieldConfirmPassword_CompareErrorMessage")]
        public string ConfirmPassword { get; set; }
    }

    //public class AddPhoneNumberViewModel
    //{
    //    [Required]
    //    [Phone]
    //    [Display(Name = "Phone Number")]
    //    public string Number { get; set; }
    //}

    //public class VerifyPhoneNumberViewModel
    //{
    //    [Required]
    //    [Display(Name = "Code")]
    //    public string Code { get; set; }

    //    [Required]
    //    [Phone]
    //    [Display(Name = "Phone Number")]
    //    public string PhoneNumber { get; set; }
    //}

    //public class ConfigureTwoFactorViewModel
    //{
    //    public string SelectedProvider { get; set; }
    //    public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    //}
}