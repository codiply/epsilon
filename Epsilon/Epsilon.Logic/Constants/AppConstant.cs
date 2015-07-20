using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants
{
    public static class AppConstant
    {
        public const string ANONYMOUS_USER_HOME_CONTROLLER = "Home";
        public const string ANONYMOUS_USER_HOME_ACTION = "Index";

        public const string AUTHENTICATED_USER_HOME_CONTROLLER = "UserHome";
        public const string AUTHENTICATED_USER_HOME_ACTION = "Index";

        public const string COUNTRY_VARIANT_RESOURCES_STEM = "Epsilon.Resources.CountryVariants.Resources";

        public const string COUNTRY_DISPLAY_FIELD = "EnglishName";
        public static Func<Country, string> COUNTRY_DISPLAY_FIELD_SELECTOR = (x => x.EnglishName);

        public const byte IP_ADDRESS_MAX_LENGTH = 39;

        public const byte TOKEN_AMOUNT_PRECISION = 16;
        public const byte TOKEN_AMOUNT_SCALE = 4;
    }
}
