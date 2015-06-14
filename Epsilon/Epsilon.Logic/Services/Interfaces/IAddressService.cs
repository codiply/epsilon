using Epsilon.Logic.Forms;
using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IAddressService
    {
        Task<IList<Country>> GetAvailableCountries();

        Task<Address> CreateOrFindAddress(AddressForm dto);
    }
}
