using Epsilon.Logic.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Helpers
{
    public class CsvHelper : ICsvHelper
    {
        public void Write(TextWriter stream, IEnumerable<IEnumerable<string>> values, IEnumerable<string> header = null)
        {
            if (header == null && values == null)
                return;

            var expectedNumberOfColumns = (values == null || !values.Any()) ? header.Count() : values.First().Count();

            if (header != null)
            {
                stream.WriteLine(ConstructLine(header, expectedNumberOfColumns));
            }
            if (values == null)
                return;

            foreach (var row in values)
            {
                stream.WriteLine(ConstructLine(row, expectedNumberOfColumns));
            }
        }

        private string ConstructLine(IEnumerable<string> values, int expectedNumberOfColumns)
        {
            var valuesArray = values as string[] ?? values.ToArray();

            var actualNumberOfColumns = valuesArray.Count();
            if (actualNumberOfColumns != expectedNumberOfColumns)
            {
                throw new Exception(string.Format(
                    "Unexpected number of columns while constructing a csv line. Expected {0} and was given {1}.",
                    expectedNumberOfColumns, actualNumberOfColumns));
            }
            if (actualNumberOfColumns == 0)
                return "";

            return string.Join(",", valuesArray.Select(QuoteIfNecessary));
        }

        private string QuoteIfNecessary(string text)
        {
            if (NeedsQuoting(text))
            {
                // Represent double quotes with 2 double quotes
                // and wrap the whole text in double quotes.
                return "\"" + text.Replace("\"", "\"\"") + "\"";
            }

            return text;
        }

        private bool NeedsQuoting(string text)
        {
            return text.Contains(",") ||
                   text.Contains("\n") ||
                   text.Contains("\"");
        }
    }
}
