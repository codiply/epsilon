using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Forms.Submission;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.Services;
using Epsilon.Resources.Logic.Address;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Services
{
    [TestFixture]
    public class AddressServiceTest
    {
        #region AddAddress

        [Test]
        public async Task AddAddress_GlobalSwitchWorks()
        {
            var mockAddressServiceConfig = new Mock<IAddressServiceConfig>();
            mockAddressServiceConfig.Setup(x => x.GlobalSwitch_DisableAddAddress).Returns(true);
            var service = new AddressService(null, null, mockAddressServiceConfig.Object, null, null, null);

            var outcome = await service.AddAddress("some-user", "some-ip-address", null);

            Assert.IsTrue(outcome.IsRejected, "Outcome field IsRejected should be true.");
            Assert.AreEqual(AddressResources.GlobalSwitch_AddAddressDisabled_Message, outcome.RejectionReason,
                "Outcome field RejectionReason is not the expected.");
            Assert.IsFalse(outcome.ReturnToForm, "Outcome field ReturnToForm should be false.");
        }

        #endregion

        #region CalculateDistinctAddressCode

        [Test]
        public void CalculateDistinctAddressCode_GB_MultipleHouseNumbers_Test()
        {
            var service = new AddressService(null, null, null, null, null, null);

            var form = new AddressForm
            {
                CountryId = EnumsHelper.CountryId.ToString(CountryId.GB),
                Postcode = "E123A",
                Line1 = "1D Somewhere 23"
            };

            var distinctAddressCode = service.CalculateDistinctAddressCode(form);
            Assert.IsNullOrEmpty(distinctAddressCode, "DistinctAddressCode is not the expected.");
        }

        [Test]
        public void CalculateDistinctAddressCode_GB_Success_Test()
        {
            var service = new AddressService(null, null, null, null, null, null);

            var form = new AddressForm
            {
                CountryId = EnumsHelper.CountryId.ToString(CountryId.GB),
                Postcode = "E123A",
                Line1 = "1D Somewhere"
            };

            var distinctAddressCode = service.CalculateDistinctAddressCode(form);
            Assert.AreEqual(distinctAddressCode, "GBE123A1D", "DistinctAddressCode is not the expected.");
        }

        public void CalculateDistinctAddressCode_GB_NoHouseNumber_Test()
        {
            var service = new AddressService(null, null, null, null, null, null);

            var form = new AddressForm
            {
                CountryId = EnumsHelper.CountryId.ToString(CountryId.GB),
                Postcode = "E123A",
                Line1 = "Somewhere"
            };

            var distinctAddressCode = service.CalculateDistinctAddressCode(form);
            Assert.IsNullOrEmpty(distinctAddressCode, "DistinctAddressCode is not the expected.");
        }

        public void CalculateDistinctAddressCode_GB_NumberNotInBeginningOfWord_Test()
        {
            var service = new AddressService(null, null, null, null, null, null);

            var form = new AddressForm
            {
                CountryId = EnumsHelper.CountryId.ToString(CountryId.GB),
                Postcode = "E123A",
                Line1 = "D123 Somewhere"
            };

            var distinctAddressCode = service.CalculateDistinctAddressCode(form);
            Assert.IsNullOrEmpty(distinctAddressCode, "DistinctAddressCode is not the expected.");
        }

        #endregion
    }
}
