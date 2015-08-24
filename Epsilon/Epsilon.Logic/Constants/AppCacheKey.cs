namespace Epsilon.Logic.Constants
{
    public static class AppCacheKey
    {
        public const string AVAILABLE_COUNTRIES = "EPSILON:APPCACHE:AVAILABLE_COUNTRIES";
        public const string AVAILABLE_LANGUAGES = "EPSILON:APPCACHE:AVAILABLE_LANGUAGES";
        public const string AVAILABLE_AND_UNAVAILABLE_LANGUAGES = "EPSILON:APPCACHE:AVAILABLE_AND_UNAVAILABLE_LANGUAGES";
        public const string COUNTRIES_DICTIONARY = "EPSILON:APPCACHE:COUNTRIES_DICTIONARY";
        public const string CURRENCIES_DICTIONARY = "EPSILON:APPCACHE:CURRENCIES_DICTIONARY";
        public const string LANGUAGES_DICTIONARY = "EPSILON:APPCACHE:LANGUAGES_DICTIONARY";
        public const string DB_APP_SETTINGS = "EPSILON:APPCACHE:DB_APP_SETTINGS";
        public const string CURRENT_TOKEN_REWARD_SCHEME = "EPSILON:APPCACHE:CURRENT_TOKEN_REWARD_SCHEME";

        public static string AdminAlertSent(string key)
        {
            return string.Format("EPSILON:APPCACHE:ADMIN_ALERT_SENT:{0}", key.ToLowerInvariant());
        }

        public static string GetGeoipInfoForIpAddress(string ipAddress)
        {
            return string.Format("EPSILON:APPCACHE:GET_GEOIP_INFO_FOR_IP_ADDRESS:{0}", ipAddress.ToLowerInvariant());
        }

        public static string GetUserExploredPropertiesSummary(string userId, bool limitItemsReturned)
        {
            return string.Format("EPSILON:APPCACHE:GET_USER_EXPLORED_PROPERTIES_SUMMARY:{0}:LIMIT_ITEMS_RETURNED:{1}",
                userId, limitItemsReturned ? "TRUE" : "FALSE");
        }

        public static string GetUserInterfaceCustomisation(string userId)
        {
            return string.Format("EPSILON:APPCACHE:GET_USER_INTERFACE_CUSTOMISATION:{0}", userId.ToLowerInvariant());
        }

        public static string GetUserOutgoingVerificationsSummary(string userId, bool limitItemsReturned)
        {
            return string.Format("EPSILON:APPCACHE:GET_USER_OUTGOING_VERIFICATIONS_SUMMARY:{0}:LIMIT_ITEMS_RETURNED:{1}",
                userId, limitItemsReturned ? "TRUE" : "FALSE");
        }

        public static string GetUserSubmissionsSummary(string userId, bool limitItemsReturned)
        {
            return string.Format("EPSILON:APPCACHE:GET_USER_SUBMISSIONS_SUMMARY:{0}:LIMIT_ITEMS_RETURNED:{1}",
                userId, limitItemsReturned ? "TRUE" : "FALSE");
        }

        public static string Language(string languageId)
        {
            return "EPSILON:APPCACHE:LANGUAGE:" + languageId.ToLowerInvariant();
        }

        public static string TextResourceHelperAllResources(string cultureCode)
        {
            return "EPSILON:APPCACHE:TEXT_RESOURCE_HELPER_ALL_RESOURCES:" + cultureCode;
        }


        public static string UserPreference(string userId)
        {
            return "EPSILON:APPCACHE:USER_PREFERENCE:" + userId.ToLowerInvariant();
        }

        public static string UserTokenBalance(string userId)
        {
            return "EPSILON:APPCACHE:USER_TOKEN_BALANCE:" + userId.ToLowerInvariant();
        }
    }
}
