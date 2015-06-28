using Epsilon.Logic.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers.Interfaces
{
    public interface IAddressCleansingHelper
    {
        AddressForm CleanseForVerification(AddressForm address);

        AddressForm CleanseForStorage(AddressForm address);
    }
}
