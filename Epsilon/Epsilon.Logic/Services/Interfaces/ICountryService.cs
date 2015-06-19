using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface ICountryService
    {
        IList<Country> GetAvailableCountries();
    }
}
