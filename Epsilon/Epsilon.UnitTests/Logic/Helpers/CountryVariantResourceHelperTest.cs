using Epsilon.Logic.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
