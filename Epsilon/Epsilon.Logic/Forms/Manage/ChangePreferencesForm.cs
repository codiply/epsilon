using Epsilon.Logic.Entities;
using Epsilon.Resources.Web.Manage;
using System.ComponentModel.DataAnnotations;

namespace Epsilon.Logic.Forms.Manage
{
    public class ChangePreferencesForm
    {
        [Required(ErrorMessageResourceType = typeof(ManageResources), ErrorMessageResourceName = "ChangePreferences_FieldLanguageId_RequiredErrorMessage")]
        [Display(ResourceType = typeof(ManageResources), Name = "ChangePreferences_FieldLanguageId_DisplayName")]
        public string LanguageId { get; set; }


        public static ChangePreferencesForm FromEntity(UserPreference entity)
        {
            return new ChangePreferencesForm
            {
                LanguageId = entity.LanguageId
            };
        }
    }
}
