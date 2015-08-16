using Epsilon.Logic.Helpers.Interfaces;
using Epsilon.Logic.Infrastructure.Primitives;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

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

        public Frequency ParseFrequency(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var regex = new Regex(@"^([0-9]+)\/([0-9]*\.?[0-9]*)([SsMmHhDd])$");
                var match = regex.Match(value);
                if (match.Success)
                {
                    var times = ParseInt(match.Groups[1].Value).Value;
                    var periodValue = 
                        string.IsNullOrWhiteSpace(match.Groups[2].Value) ? 1.0 : ParseFloat(match.Groups[2].Value).Value;
                    var period = Period(periodValue, match.Groups[3].Value);
                    return new Frequency(times, period);
                }
            }
            return null;
        }

        private TimeSpan Period(double value, string unit)
        {
            unit = unit.ToUpperInvariant();
            if (unit.Equals("S"))
            {
                return TimeSpan.FromSeconds(value);
            }
            else if (unit.Equals("M"))
            {
                return TimeSpan.FromMinutes(value);
            }
            else if (unit.Equals("H"))
            {
                return TimeSpan.FromHours(value);
            }
            else if (unit.Equals("D"))
            {
                return TimeSpan.FromDays(value);
            }

            throw new ArgumentException(string.Format("Unexpected period unit '{0}'", unit));
        }
    }
}
