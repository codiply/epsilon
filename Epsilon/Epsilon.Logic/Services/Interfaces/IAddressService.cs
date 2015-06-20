using Epsilon.Logic.Forms;
using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epsilon.Logic.JsonModels;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface IAddressService
    {
        Task<IList<TypeaheadAddressResult>> TypeaheadSearch(TypeaheadAddressRequest request);

        Task<Address> AddAddress(AddressForm dto);
    }
}
