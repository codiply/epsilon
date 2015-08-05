using Epsilon.Web.Models.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Web.Models.ViewModels.PropertyInfo
{
    class GainAccessViewModel
    {
        public Guid AccessUniqueId { get; set; }
        public AddressDetailsViewModel AddressDetails { get; set; }
    }
}
