using Epsilon.Resources.Common;
using Epsilon.Resources.Web.Account;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Epsilon.Web.Models
{
    //public class ExternalLoginConfirmationViewModel
    //{
    //    [Required]
    //    [Display(Name = "Email")]
    //    public string Email { get; set; }
    //}

    //public class ExternalLoginListViewModel
    //{
    //    public string ReturnUrl { get; set; }
    //}

    //public class SendCodeViewModel
    //{
    //    public string SelectedProvider { get; set; }
    //    public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    //    public string ReturnUrl { get; set; }
    //    public bool RememberMe { get; set; }
    //}

    //public class VerifyCodeViewModel
    //{
    //    [Required]
    //    public string Provider { get; set; }

    //    [Required]
    //    [Display(Name = "Code")]
    //    public string Code { get; set; }
    //    public string ReturnUrl { get; set; }

    //    [Display(Name = "Remember this browser?")]
    //    public bool RememberBrowser { get; set; }

    //    public bool RememberMe { get; set; }
    //}

    public class ForgotViewModel
    {
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "FieldEmail_RequiredErrorMessage")]
        [EmailAddress]
        [Display(ResourceType = typeof(AccountResources), Name = "FieldEmail_DisplayName")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "FieldEmail_RequiredErrorMessage")]
        [EmailAddress]
        [Display(ResourceType = typeof(AccountResources), Name = "FieldEmail_DisplayName")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "FieldPassword_RequiredErrorMessage")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AccountResources), Name = "FieldPassword_DisplayName")]
        public string Password { get; set; }

        [Display(ResourceType = typeof(AccountResources), Name = "FieldRememberMe_DisplayName")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "FieldEmail_RequiredErrorMessage")]
        [EmailAddress]
        [Display(ResourceType = typeof(AccountResources), Name = "FieldEmail_DisplayName")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "FieldPassword_RequiredErrorMessage")]
        [StringLength(100,
                    ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "StringLengthErrorMessage",
                    MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AccountResources), Name = "FieldPassword_DisplayName")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AccountResources), Name = "FieldConfirmPassword_DisplayName")]
        [Compare("Password", 
            ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "FieldConfirmPassword_CompareErrorMessage")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "FieldEmail_RequiredErrorMessage")]
        [EmailAddress]
        [Display(ResourceType = typeof(AccountResources), Name = "FieldEmail_DisplayName")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "FieldPassword_RequiredErrorMessage")]
        [StringLength(100,
            ErrorMessageResourceType = typeof(CommonResources), ErrorMessageResourceName = "StringLengthErrorMessage", 
            MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AccountResources), Name = "FieldPassword_DisplayName")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", 
            ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "FieldConfirmPassword_CompareErrorMessage")]
        [Display(ResourceType = typeof(AccountResources), Name = "FieldConfirmPassword_DisplayName")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "FieldEmail_RequiredErrorMessage")]
        [EmailAddress]
        [Display(ResourceType = typeof(AccountResources), Name = "FieldEmail_DisplayName")]
        public string Email { get; set; }
    }
}
