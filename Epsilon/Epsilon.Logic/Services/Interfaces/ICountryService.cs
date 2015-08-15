using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Entities;
using System.Collections.Generic;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface ICountryService
    {
        string GetDisplayName(string countryId);

        IList<Country> GetAvailableCountries();

        Country GetCountry(string countryId);

        bool IsCountryAvailable(CountryId countryId);
    }
}
