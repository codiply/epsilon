using Epsilon.Logic.Helpers;
using NUnit.Framework;

namespace Epsilon.UnitTests.Logic.Helpers
{
    [TestFixture]
    public class CountryVariantResourceHelperTest
    {
        private readonly CountryVariantResourceHelper _helper = new CountryVariantResourceHelper();

        [Test]
        public void GetVariantsTest_ForSingleResource()
        {
            var resourceNames = EnumsHelper.CountryVariantResourceName.GetValues();

            foreach (var name in resourceNames)
            {
                var variants = _helper.GetVariants(name);
                foreach (var countryId in EnumsHelper.CountryId.GetNames())
                {
                    var variant = variants[countryId];
                    Assert.IsNotNull(variant,
                        string.Format("Variant for key '{0}' and CountryId '{1}' is null. Add key and leave it empty instead.",
                            EnumsHelper.CountryVariantResourceName.ToString(name), countryId));
                }
            }
        }

        [Test]
        public void GetVariantsTest_ForSeveralResources()
        {
            var resourceNames = EnumsHelper.CountryVariantResourceName.GetValues();

            var variants = _helper.GetVariants(resourceNames);
            foreach (var countryId in EnumsHelper.CountryId.GetNames())
            {
                var countryVariants = variants[countryId];
                foreach (var name in resourceNames)
                {
                    var variant = countryVariants[EnumsHelper.CountryVariantResourceName.ToString(name)];
                    Assert.IsNotNull(variant,
                        string.Format("Variant for key '{0}' and CountryId '{1}' is null. Add key and leave it empty instead.",
                        EnumsHelper.CountryVariantResourceName.ToString(name), countryId));
                }
            }
        }

        [Test]
        public void GetVariantsForCountry_AllResourcesAreDefinedForAllCountries()
        {
            var allCountryIds = EnumsHelper.CountryId.GetNames();
            var allCountryVariantResourceNameEnums = EnumsHelper.CountryVariantResourceName.GetValues();
            var allCountryVariantResourceNames = EnumsHelper.CountryVariantResourceName.GetNames();

            {
                foreach (var countryId in allCountryIds)
                {
                    var variants = _helper.GetVariantsForCountry(countryId, allCountryVariantResourceNameEnums);

                    Assert.AreEqual(allCountryVariantResourceNames.Count, variants.Count,
                        string.Format("Variants for countryId '{0}' do not have the expected Count.", countryId));

                    foreach (var resourceName in allCountryVariantResourceNames)
                    {
                        Assert.IsTrue(variants.ContainsKey(resourceName),
                            "Resource '{0}' was not found for countryId {1}.", resourceName, countryId);
                    }
                }
            }
        }
    }
}
