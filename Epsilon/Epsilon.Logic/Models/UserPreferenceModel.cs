using Epsilon.Logic.Constants;
using Epsilon.Logic.Entities;

namespace Epsilon.Logic.Models
{
    public class UserPreferenceModel
    {
        public string LanguageId { get; set; }
        public string Language { get; set; }

        public static UserPreferenceModel FromEntity(UserPreference entity)
        {
            return new UserPreferenceModel
            {
                LanguageId = entity.LanguageId,
                Language = AppConstant.LANGUAGE_DISPLAY_FIELD_SELECTOR(entity.Language)
            };
        }
    }
}
