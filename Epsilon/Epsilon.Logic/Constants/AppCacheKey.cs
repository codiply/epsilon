using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants
{
    public static class AppCacheKey
    {
        public const string AVAILABLE_COUNTRIES = "APPCACHE:AVAILABLE_COUNTRIES";
        public const string AVAILABLE_LANGUAGES = "APPCACHE:AVAILABLE_LANGUAGES";
        public const string COUNTRIES_DICTIONARY = "APPCACHE:COUNTRIES_DICTIONARY";
        public const string LANGUAGES_DICTIONARY = "APPCACHE:LANGUAGES_DICTIONARY";
        public const string DB_APP_SETTINGS = "APPCACHE:DB_APP_SETTINGS";
        public const string CURRENT_TOKEN_REWARD_SCHEME = "APPCACHE:CURRENT_TOKEN_REWARD_SCHEME";


        public static string Language(string languageId)
        {
            return "APPCACHE:LANGUAGE:" + languageId.ToLowerInvariant();
        }

        public static string UserPreference(string userId)
        {
            return "APPCACHE:USER_PREFERENCE:" + userId.ToLowerInvariant();
        }

        public static string UserTokenBalance(string userId)
        {
            return "APPCACHE:USER_TOKEN_BALANCE:" + userId.ToLowerInvariant();
        }
    }
}
