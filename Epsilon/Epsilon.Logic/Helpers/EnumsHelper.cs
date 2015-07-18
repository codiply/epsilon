using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers
{
    public static class EnumsHelper
    {
        public static EnumMemoizer<CoinAccountTransactionTypeId> CoinAccountTransactionTypeId =
            new EnumMemoizer<Constants.Enums.CoinAccountTransactionTypeId>();

        public static EnumMemoizer<CountryId> CountryId = new EnumMemoizer<CountryId>();

        public static EnumMemoizer<DbAppSettingValueType> DbAppSettingValueType =
            new EnumMemoizer<DbAppSettingValueType>();

        public static EnumMemoizer<IpAddressActivityType> IpAddressActivityType =
            new EnumMemoizer<IpAddressActivityType>();

        public static EnumMemoizer<CountryVariantResourceName> CountryVariantResourceName =
            new EnumMemoizer<CountryVariantResourceName>();

        public static EnumMemoizer<GeocodeFailureType> GeocodeFailureType =
            new EnumMemoizer<GeocodeFailureType>();

        public static EnumMemoizer<GeocodeQueryType> GeocodeQueryType =
            new EnumMemoizer<GeocodeQueryType>();
    }
}
