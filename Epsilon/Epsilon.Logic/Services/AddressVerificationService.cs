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
        private readonly IAddressCleansingHelper _addressCleansingHelper;

        public AddressVerificationService(
            IAddressCleansingHelper addressCleansingHelper)
        {
            _addressCleansingHelper = addressCleansingHelper;
        }

        public async Task<AddressVerificationResponse> Verify(AddressForm address)
        {
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
