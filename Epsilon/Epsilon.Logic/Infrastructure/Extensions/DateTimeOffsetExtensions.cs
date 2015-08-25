using System;
using System.Globalization;

namespace Epsilon.Logic.Infrastructure.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        public static string ToStringUnambiguous(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToString("yyyy-MM-ddThh:mm:ss.FFFFFzzz", CultureInfo.InvariantCulture);
        }

        public static string ToStringUnambiguous(this DateTimeOffset? dateTimeOffset)
        {
            if (dateTimeOffset.HasValue)
                return dateTimeOffset.Value.ToStringUnambiguous();
            else
                return "";
        }
    }
}
