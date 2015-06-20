using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers.Interfaces
{
    public interface IParseHelper
    {
        int? ParseInt(string value);

        long? ParseLong(string value);

        float? ParseFloat(string value);

        double? ParseDouble(string value);

        decimal? ParseDecimal(string value);

        bool? ParseBool(string value);

        Guid? ParseGuid(string value);

        TimeSpan? ParseTimeSpan(string value);
    }
}
