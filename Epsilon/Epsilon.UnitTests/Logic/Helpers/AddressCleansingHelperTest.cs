using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Forms.Submission;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Helpers.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Helpers
{
    [TestFixture]
    public class AddressCleansingHelperTest
    {
        private readonly IAddressCleansingHelper _helper = new AddressCleansingHelper();

        [Test]
        public void CleanseTest()
        {
            var address = new AddressForm
            {
                Line1 = " Line1 ",
                Line2 = " Line2 ",
                Line3 = " Line3 ",
                Line4 = " Line4 ",
                Locality = " Locality ",
                Region = " Region ",
                Postcode = " x1 2ab ",
                CountryId = "GB",
                UniqueId = Guid.NewGuid()
            };

            var cleansedAddress = _helper.Cleanse(address);

            Assert.AreEqual("Line1", cleansedAddress.Line1, "Line1 in cleansed address is not the expected.");
            Assert.AreEqual("Line2", cleansedAddress.Line2, "Line2 in cleansed address is not the expected.");
            Assert.AreEqual("Line3", cleansedAddress.Line3, "Line3 in cleansed address is not the expected.");
            Assert.AreEqual("Line4", cleansedAddress.Line4, "Line4 in cleansed address is not the expected.");
            Assert.AreEqual("Locality", cleansedAddress.Locality, "Locality in cleansed address is not the expected.");
            Assert.AreEqual("Region", cleansedAddress.Region, "Region in cleansed address is not the expected.");
            Assert.AreEqual("X12AB", cleansedAddress.Postcode, "Postcode in cleansed address is not the expected.");
            Assert.AreEqual("GB", cleansedAddress.CountryId, "CountryId in cleansed address is not the expected.");
            Assert.AreEqual(address.UniqueId, cleansedAddress.UniqueId,"The UniqueId is not the expected.");
        }

        [Test]
        public void CleansePostcode_GB_Test()
        {
            var countryId = CountryId.GB;

            var postcode1 = " x1 2ab";
            var expectedCleanPostcode1 = "X12AB";
            Assert.AreEqual(expectedCleanPostcode1, _helper.CleansePostcode(countryId, postcode1),
                "Test failed for postcode 1.");
        }

        [Test]
        public void CleansePostcode_GR_Test()
        {
            var countryId = CountryId.GR;

            var postcode1 = " 12345 ";
            var expectedCleanPostcode1 = "12345";
            Assert.AreEqual(expectedCleanPostcode1, _helper.CleansePostcode(countryId, postcode1),
                "Test failed for postcode 1.");
        }
    }
}
