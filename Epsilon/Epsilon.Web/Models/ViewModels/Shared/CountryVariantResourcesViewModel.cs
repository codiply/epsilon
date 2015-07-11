using Epsilon.Logic.Constants.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Web.Models.ViewModels.Shared
{
    public class CountryVariantResourcesViewModel
    {
        public string Constant { get; set; }
        public List<CountryVariantResourceName> ResourceNames { get; set; }
    }
}
