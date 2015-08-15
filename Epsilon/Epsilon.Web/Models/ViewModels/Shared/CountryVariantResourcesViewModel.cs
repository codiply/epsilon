using Epsilon.Logic.Constants.Enums;
using System.Collections.Generic;

namespace Epsilon.Web.Models.ViewModels.Shared
{
    public class CountryVariantResourcesViewModel
    {
        public string Constant { get; set; }
        public List<CountryVariantResourceName> ResourceNames { get; set; }
    }
}
