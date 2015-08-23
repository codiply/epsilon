using Epsilon.Logic.Entities;
using System;

namespace Epsilon.Logic.Constants
{
    public static class AppConstant
    {
        public const string ANONYMOUS_USER_HOME_CONTROLLER = "Home";
        public const string ANONYMOUS_USER_HOME_ACTION = "Index";

        public const string AUTHENTICATED_USER_HOME_CONTROLLER = "UserHome";
        public const string AUTHENTICATED_USER_HOME_ACTION = "Index";

        public const string CONTENT_TYPE_CSV = "text/csv";

        public const string COUNTRY_VARIANT_RESOURCES_STEM = "Epsilon.Resources.CountryVariants.Resources";

        public const string COUNTRY_DISPLAY_FIELD = "LocalName";
        public static Func<Country, string> COUNTRY_DISPLAY_FIELD_SELECTOR = (x => string.Format("{0} - {1}", x.EnglishName, x.LocalName));

        public static Func<Currency, string> CURRENCY_DISPLAY_FIELD_SELECTOR = (x => string.Format("{0} - {1}", x.EnglishName, x.LocalName));

        public const byte IP_ADDRESS_MAX_LENGTH = 39;

        public const string LANGUAGE_DISPLAY_FIELD = "LocalName";
        public static Func<Language, string> LANGUAGE_DISPLAY_FIELD_SELECTOR = (x => string.Format("{0} - {1}", x.EnglishName, x.LocalName));


        public const int MAX_NUMBERS_OF_BEDROOMS = 20;

        public const byte SECRET_CODE_MAX_LENGTH = 8;

        public const byte TOKEN_AMOUNT_PRECISION = 16;
        public const byte TOKEN_AMOUNT_SCALE = 4;
        public const decimal TOKEN_REWARD_DELTA = 0.0001M;

        public const byte RATING_MIN_VALUE = 1;
        public const byte RATING_MAX_VALUE = 5;

        public const string TOKEN_REWARD_KEY_EARN = "Earn";
        public const string TOKEN_REWARD_KEY_SPEND = "Spend";

        public const string GEOCODE_QUERY_TYPE_POSTCODE = "postcode";
        public const string GEOCODE_QUERY_TYPE_ADDRESS = "address";
    }
}
