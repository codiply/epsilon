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
        public const string CURRENCIES_DICTIONARY = "APPCACHE:CURRENCIES_DICTIONARY";
        public const string LANGUAGES_DICTIONARY = "APPCACHE:LANGUAGES_DICTIONARY";
        public const string DB_APP_SETTINGS = "APPCACHE:DB_APP_SETTINGS";
        public const string CURRENT_TOKEN_REWARD_SCHEME = "APPCACHE:CURRENT_TOKEN_REWARD_SCHEME";

        public static string GetGeoipInfoForIpAddress(string ipAddress)
        {
            return string.Format("APPCACHE:GET_GEOIP_INFO_FOR_IP_ADDRESS:{0}", ipAddress);
        }

        public static string GetUserExploredPropertiesSummary(string userId, bool limitItemsReturned)
        {
            return string.Format("APPCACHE:GET_USER_EXPLORED_PROPERTIES_SUMMARY:{0}:LIMIT_ITEMS_RETURNED:{1}",
                userId, limitItemsReturned ? "TRUE" : "FALSE");
        }

        public static string GetUserInterfaceCustomisationForUser(string userId)
        {
            return string.Format("APPCACHE:GET_USER_INTERFACE_CUSTOMISATION_FOR_USER:{0}", userId);
        }

        public static string GetUserOutgoingVerificationsSummary(string userId, bool limitItemsReturned)
        {
            return string.Format("APPCACHE:GET_USER_OUTGOING_VERIFICATIONS_SUMMARY:{0}:LIMIT_ITEMS_RETURNED:{1}",
                userId, limitItemsReturned ? "TRUE" : "FALSE");
        }

        public static string GetUserSubmissionsSummary(string userId, bool limitItemsReturned)
        {
            return string.Format("APPCACHE:GET_USER_SUBMISSIONS_SUMMARY:{0}:LIMIT_ITEMS_RETURNED:{1}",
                userId, limitItemsReturned ? "TRUE" : "FALSE");
        }

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
