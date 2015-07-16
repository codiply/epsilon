using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Forms;
using Epsilon.Logic.FSharp;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class AddressVerificationService : IAddressVerificationService
    {
        private readonly IAddressVerificationServiceConfig _addressVerificationServiceConfig;
        private readonly IAddressCleansingHelper _addressCleansingHelper;

        public AddressVerificationService(
            IAddressVerificationServiceConfig addressVerificationServiceConfig,
            IAddressCleansingHelper addressCleansingHelper)
        {
            _addressVerificationServiceConfig = addressVerificationServiceConfig;
            _addressCleansingHelper = addressCleansingHelper;
        }

        public async Task<AddressVerificationResponse> Verify(AddressForm address)
        {
            var allWords = address.AllWords();
            //var response = await Google.geocode(allWords, "");
            var cleansedAddress = _addressCleansingHelper.CleanseForVerification(address);
            return new AddressVerificationResponse
            {
                IsRejected = false,
                Latitude = 0.0M, // TODO_PANOS
                Longitude = 0.0M // TODO_PANOS
            };
        }
    }
}
