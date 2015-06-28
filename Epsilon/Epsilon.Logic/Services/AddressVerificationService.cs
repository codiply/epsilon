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
            throw new NotImplementedException();
        }
    }
}
