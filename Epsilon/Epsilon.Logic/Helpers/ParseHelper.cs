using Epsilon.Logic.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers
{
    public class ParseHelper : IParseHelper
    {
        public int? ParseInt(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                int answer;
                if (int.TryParse(value, out answer))
                {
                    return answer;
                }
            }

            return null;
        }

        public long? ParseLong(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                long answer;
                if (long.TryParse(value, out answer))
                {
                    return answer;
                }
            }

            return null;
        }

        public float? ParseFloat(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                float answer;
                if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out answer))
                {
                    return answer;
                }
            }

            return null;
        }

        public double? ParseDouble(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                double answer;
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out answer))
                {
                    return answer;
                }
            }

            return null;
        }

        public decimal? ParseDecimal(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                decimal answer;
                if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out answer))
                {
                    return answer;
                }
            }

            return null;
        }

        public bool? ParseBool(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                bool answer;
                if (bool.TryParse(value, out answer))
                {
                    return answer;
                }
            }

            return null;
        }

        public Guid? ParseGuid(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Guid answer;
                if (Guid.TryParse(value, out answer))
                {
                    return answer;
                }
            }

            return null;
        }

        public TimeSpan? ParseTimeSpan(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                TimeSpan answer;
                if (TimeSpan.TryParse(value, out answer))
                {
                    return answer;
                }
            }

            return null;
        }

        public DateTime? ParseDateTime(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                DateTime answer;
                if (DateTime.TryParse(value, out answer))
                {
                    return answer;
                }
            }

            return null;
        }

        public DateTimeOffset? ParseDateTimeOffset(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                DateTimeOffset answer;
                if (DateTimeOffset.TryParse(value, out answer))
                {
                    return answer;
                }
            }

            return null;
        }
    }
}
