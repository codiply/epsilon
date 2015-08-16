using Epsilon.Web.Models.ViewModels.Shared;
using System;

namespace Epsilon.Web.Models.ViewModels.PropertyInfo
{
    public class GainAccessViewModel
    {
        public Guid AccessUniqueId { get; set; }
        public AddressDetailsViewModel AddressDetails { get; set; }
        public decimal TokensCost { get; set; }
        public Guid? ExistingUnexpiredAccessUniqueId { get; set; }
    }
}
