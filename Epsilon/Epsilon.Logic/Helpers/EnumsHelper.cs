using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Infrastructure;

namespace Epsilon.Logic.Helpers
{
    public static class EnumsHelper
    {
        public static EnumMemoizer<AdminEventLogKey> AdminEventLogKey =
            new EnumMemoizer<AdminEventLogKey>();

        public static EnumMemoizer<CountryId> CountryId = new EnumMemoizer<CountryId>();

        public static EnumMemoizer<CurrencyId> CurrencyId = new EnumMemoizer<CurrencyId>();

        public static EnumMemoizer<CountryVariantResourceName> CountryVariantResourceName =
            new EnumMemoizer<CountryVariantResourceName>();

        public static EnumMemoizer<DbAppSettingKey> DbAppSettingKey =
            new EnumMemoizer<DbAppSettingKey>();

        public static EnumMemoizer<DbAppSettingValueType> DbAppSettingValueType =
            new EnumMemoizer<DbAppSettingValueType>();

        public static EnumMemoizer<GeocodeAddressStatus> GeocodeAddressStatus =
            new EnumMemoizer<GeocodeAddressStatus>();

        public static EnumMemoizer<GeocodePostcodeStatus> GeocodePostcodeStatus =
            new EnumMemoizer<GeocodePostcodeStatus>();

        public static EnumMemoizer<GeocodeFailureType> GeocodeFailureType =
            new EnumMemoizer<GeocodeFailureType>();

        public static EnumMemoizer<GeocodeQueryType> GeocodeQueryType =
            new EnumMemoizer<GeocodeQueryType>();

        public static EnumMemoizer<GeoipProviderName> GeoipProviderName =
            new EnumMemoizer<GeoipProviderName>();

        public static EnumMemoizer<IpAddressActivityType> IpAddressActivityType =
            new EnumMemoizer<IpAddressActivityType>();

        public static EnumMemoizer<TokenRewardKey> TokenRewardKey =
            new EnumMemoizer<TokenRewardKey>();

        public static EnumMemoizer<WebClientResponseStatus> WebClientResponseStatus =
            new EnumMemoizer<WebClientResponseStatus>();
    }
}
