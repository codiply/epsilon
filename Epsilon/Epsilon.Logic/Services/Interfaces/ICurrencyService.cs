using Epsilon.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Services.Interfaces
{
    public interface ICurrencyService
    {
        IList<Currency> GetAll();

        string GetSymbol(string currencyId);

        string GetDisplayName(string currencyId);
    }
}
