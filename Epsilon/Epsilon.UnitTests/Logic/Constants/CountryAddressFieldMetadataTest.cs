using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Constants.Interfaces.CountryAddressFieldMetadata;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Constants
{
    [TestFixture]
    public class CountryAddressFieldMetadataTest
    {
        private static Dictionary<string, Func<CountryMetadata, FieldMetadata>> allFieldMappings = new Dictionary<string, Func<CountryMetadata, FieldMetadata>>
        {
            { "Line1", x => x.Line1 },
            { "Line2", x => x.Line2 },
            { "Line3", x => x.Line3 },
            { "Line4", x => x.Line4 },
            { "Locality", x => x.Locality },
            { "Region", x => x.Region },
            { "Postcode", x => x.Postcode }
        };

        [Test]
        public void AllRequiredFielsAreMarkedAsUsed()
        {
            var allMetadata = new CountryAddressFieldMetadata();
            var allCountryIds = Enum.GetValues(typeof(CountryId)) as CountryId[];

            foreach (var countryId in allCountryIds)
            {
                var countryMetadata = allMetadata.GetForCountry(countryId);

                foreach (var field in allFieldMappings.Keys)
                {
                    var fieldMetadata = allFieldMappings[field](countryMetadata);
                    if (fieldMetadata.IsRequired)
                        Assert.IsTrue(fieldMetadata.IsUsed,
                            String.Format("Field {0} for CountryId {1} should be marked as IsUsed because it is marked as IsRequired.", 
                                field, countryId.ToString()));
                }
            }
        }

        [Test]
        public void NonNullableFieldsInTheDatabaseShouldBeMarkedRequired()
        {
            var requiredFields = new List<string> { "Line1", "Locality", "Postcode" };
            
            var allMetadata = new CountryAddressFieldMetadata();
            var allCountryIds = Enum.GetValues(typeof(CountryId)) as CountryId[];

            foreach (var countryId in allCountryIds)
            {
                var countryMetadata = allMetadata.GetForCountry(countryId);

                foreach (var field in requiredFields)
                {
                    var fieldMetadata = allFieldMappings[field](countryMetadata);
                    Assert.IsTrue(fieldMetadata.IsRequired,
                         String.Format("Field {0} for CountryId {1} should be marked as IsRequired because it is non-nullable in the database.",
                             field, countryId.ToString()));
                }
            }
        }
    }
}
