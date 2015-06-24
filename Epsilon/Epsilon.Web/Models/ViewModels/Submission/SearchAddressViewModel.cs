using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Epsilon.Web.Models.ViewModels.Submission
{
    public class SearchAddressViewModel
    {
        public IList<Country> AvailableCountries { get; set; }
    }
}