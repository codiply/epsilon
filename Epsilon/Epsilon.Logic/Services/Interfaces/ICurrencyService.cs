using Epsilon.Logic.Entities;
using System.Collections.Generic;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface ICurrencyService
    {
        IList<Currency> GetAll();

        Currency Get(string currencyId);

        string GetDisplayName(string currencyId);

        string GetSymbol(string currencyId);
    }
}
