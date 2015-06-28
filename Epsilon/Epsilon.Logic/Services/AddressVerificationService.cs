using Epsilon.Logic.Forms;
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
        public async Task<AddressVerificationResponse> Verify(AddressForm address)
        {
            // TODO_PANOS: Wrap any external service calls in a try catch
            //             and send an admin alert if they throw.

            // TODO_PANOS: Fetch and return coordinates as you use a geocoding service for verification.

            throw new NotImplementedException();
        }
    }
}
