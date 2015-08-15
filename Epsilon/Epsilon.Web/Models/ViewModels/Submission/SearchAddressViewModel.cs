using Epsilon.Logic.Entities;
using System.Collections.Generic;

namespace Epsilon.Web.Models.ViewModels.Submission
{
    public class SearchAddressViewModel
    {
        public IList<Country> AvailableCountries { get; set; }
    }
}