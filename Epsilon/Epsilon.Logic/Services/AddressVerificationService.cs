using Epsilon.Logic.Configuration.Interfaces;
using Epsilon.Logic.Forms;
using Epsilon.Logic.FSharp;
using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Logic.SqlContext.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services
{
    public class AddressVerificationService : IAddressVerificationService
    {
        private readonly IEpsilonContext _dbContext;
        private readonly IAddressCleansingHelper _addressCleansingHelper;

        public AddressVerificationService(
            IEpsilonContext dbContext,
            IAddressCleansingHelper addressCleansingHelper)
        {
            _dbContext = dbContext;
            _addressCleansingHelper = addressCleansingHelper;
        }

        public async Task<AddressVerificationResponse> Verify(string userId, string userIpAddress, AddressForm address)
        {
            var cleansedAddress = _addressCleansingHelper.CleanseForVerification(address);
            return new AddressVerificationResponse
            {
                IsRejected = false
            };
        }
    }
}
