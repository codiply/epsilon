using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Constants
{
    public class AppConstant
    {
        public const string ANONYMOUS_USER_HOME_CONTROLLER = "Home";
        public const string ANONYMOUS_USER_HOME_ACTION = "Index";

        public const string AUTHENTICATED_USER_HOME_CONTROLLER = "UserHome";
        public const string AUTHENTICATED_USER_HOME_ACTION = "Index";

        public const string COUNTRY_VARIANT_RESOURCES_STEM = "Epsilon.Resources.CountryVariants.Resources";

        public const byte IP_ADDRESS_MAX_LENGTH = 39;
    }
}
