using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants
{
    public static class AppCacheKeys
    {
        public const string AVAILABLE_COUNTRIES = "AVAILABLE_COUNTRIES";
        public const string AVAILABLE_LANGUAGES = "AVAILABLE_LANGUAGES";
        public const string COUNTRIES_DICTIONARY = "COUNTRIES_DICTIONARY";
        public const string LANGUAGES_DICTIONARY = "LANGUAGES_DICTIONARY";


        public static string Language(string languageId)
        {
            return "LANGUAGE:" + languageId.ToLowerInvariant();
        }

        public static string UserPreference(string userId)
        {
            return "USER_PREFERENCE:" + userId.ToLowerInvariant();
        }
    }
}
