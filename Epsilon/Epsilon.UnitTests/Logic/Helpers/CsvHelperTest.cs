using Epsilon.Logic.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.UnitTests.Logic.Helpers
{
    [TestFixture]
    public class CsvHelperTest
    {
        private readonly CsvHelper _csvHelper = new CsvHelper();

        [Test]
        public void Write_CanHandleEmptyValues()
        {
            // Arrange
            var header = new string[] { "Column1", "Column2" };
            var values = new List<List<string>>();

            var expectedCsv =
                "Column1,Column2\n";

            // Act
            string result = "";
            using (var sw = new StringWriter())
            {
                _csvHelper.Write(sw, values, header);
                result = sw.ToString();
            }

            // Assert
            var outcome = SameCsvText(expectedCsv, result);

            Assert.IsFalse(outcome.NumberOfLinesWrong, "The number of lines is not the expected.");
            Assert.IsTrue(outcome.IsSame,
                string.Format("Expected {0} but got {1}.", outcome.ExpectedLine, outcome.ActualLine));
        }

        [Test]
        public void Write_CanHandleNullValues()
        {
            // Arrange
            var header = new string[] { "Column1", "Column2" };
            var values = (List<List<string>>)null;

            var expectedCsv =
                "Column1,Column2\n";

            // Act
            string result = "";
            using (var sw = new StringWriter())
            {
                _csvHelper.Write(sw, values, header);
                result = sw.ToString();
            }

            // Assert
            var outcome = SameCsvText(expectedCsv, result);

            Assert.IsFalse(outcome.NumberOfLinesWrong, "The number of lines is not the expected.");
            Assert.IsTrue(outcome.IsSame,
                string.Format("Expected {0} but got {1}.", outcome.ExpectedLine, outcome.ActualLine));
        }

        [Test]
        public void Write_CanHandleNullValuesAndEmptyHeader()
        {
            // Arrange
            var header = new string[] { };
            var values = (List<List<string>>)null;

            var expectedCsv =
                "";

            // Act
            string result = "";
            using (var sw = new StringWriter())
            {
                _csvHelper.Write(sw, values, header);
                result = sw.ToString();
            }

            // Assert
            var outcome = SameCsvText(expectedCsv, result);

            Assert.IsFalse(outcome.NumberOfLinesWrong, "The number of lines is not the expected.");
            Assert.IsTrue(outcome.IsSame,
                string.Format("Expected {0} but got {1}.", outcome.ExpectedLine, outcome.ActualLine));
        }

        [Test]
        public void Write_CanHandleEmptyValuesAndEmptyHeader()
        {
            // Arrange
            var header = new string[] { };
            var values = new List<List<string>>();

            var expectedCsv =
                "";

            // Act
            string result = "";
            using (var sw = new StringWriter())
            {
                _csvHelper.Write(sw, values, header);
                result = sw.ToString();
            }

            // Assert
            var outcome = SameCsvText(expectedCsv, result);

            Assert.IsFalse(outcome.NumberOfLinesWrong, "The number of lines is not the expected.");
            Assert.IsTrue(outcome.IsSame,
                string.Format("Expected {0} but got {1}.", outcome.ExpectedLine, outcome.ActualLine));
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void Write_ThrowsIfLineSizesDoNotMatch()
        {
            // Arrange
            var values = new List<List<string>>
            {
                new List<string> { "Value11", "Value12" },
                new List<string> { "Value21" }
            };

            // Act
            string result = "";
            using (var sw = new StringWriter())
            {
                _csvHelper.Write(sw, values);
                result = sw.ToString();
            }
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void Write_ThrowsIfLineSizesDoNotMatch_WithHeader()
        {
            // Arrange
            var header = new string[] { "Column1", "Column2" };
            var values = new List<List<string>>
            {
                new List<string> { "Value11", "Value12" },
                new List<string> { "Value21" }
            };

            // Act
            string result = "";
            using (var sw = new StringWriter())
            {
                _csvHelper.Write(sw, values, header);
                result = sw.ToString();
            }
        }

        [ExpectedException(typeof(Exception))]
        public void Write_ThrowsIfLineSizesDoNotMatchHeaderSize()
        {
            // Arrange
            var header = new string[] { "Column1" };
            var values = new List<List<string>>
            {
                new List<string> { "Value11", "Value12" },
                new List<string> { "Value21", "Value22" }
            };

            // Act
            string result = "";
            using (var sw = new StringWriter())
            {
                _csvHelper.Write(sw, values, header);
                result = sw.ToString();
            }
        }

        [Test]
        public void Write_DoesNotQuoteSimpleText()
        {
            // Arrange
            var values = new List<List<string>>
            {
                new List<string> { "Value11", "Value12" },
                new List<string> { "Value21", "Value22" }
            };

            var expectedCsv =
                "Value11,Value12\n Value21,Value22\n";

            // Act
            string result = "";
            using (var sw = new StringWriter())
            {
                _csvHelper.Write(sw, values);
                result = sw.ToString();
            }

            // Assert
            var outcome = SameCsvText(expectedCsv, result);

            Assert.IsFalse(outcome.NumberOfLinesWrong, "The number of lines is not the expected.");
            Assert.IsTrue(outcome.IsSame,
                string.Format("Expected {0} but got {1}.", outcome.ExpectedLine, outcome.ActualLine));
        }

        [Test]
        public void Write_DoesNotQuoteSimpleText_WithHeader()
        {
            // Arrange
            var header = new string[] { "Column1", "Column2" };
            var values = new List<List<string>>
            {
                new List<string> { "Value11", "Value12" },
                new List<string> { "Value21", "Value22" }
            };

            var expectedCsv =
                "Column1,Column2\n Value11,Value12\n Value21,Value22\n";

            // Act
            string result = "";
            using (var sw = new StringWriter())
            {
                _csvHelper.Write(sw, values, header);
                result = sw.ToString();
            }

            // Assert
            var outcome = SameCsvText(expectedCsv, result);

            Assert.IsFalse(outcome.NumberOfLinesWrong, "The number of lines is not the expected.");
            Assert.IsTrue(outcome.IsSame,
                string.Format("Expected {0} but got {1}.", outcome.ExpectedLine, outcome.ActualLine));
        }

        [Test]
        public void Write_QuotesValuesWithCommas()
        {
            // Arrange
            var values = new List<List<string>>
            {
                new List<string> { "Value,Value11", "Value12" },
                new List<string> { "Value21", "Value22" }
            };

            var expectedCsv =
                "\"Value,Value11\",Value12\n Value21,Value22\n";

            // Act
            string result = "";
            using (var sw = new StringWriter())
            {
                _csvHelper.Write(sw, values);
                result = sw.ToString();
            }

            // Assert
            var outcome = SameCsvText(expectedCsv, result);

            Assert.IsFalse(outcome.NumberOfLinesWrong, "The number of lines is not the expected.");
            Assert.IsTrue(outcome.IsSame,
                string.Format("Expected {0} but got {1}.", outcome.ExpectedLine, outcome.ActualLine));
        }

        [Test]
        public void Write_QuotesValuesWithCommas_WithHeader()
        {
            // Arrange
            var header = new string[] { "Column,1", "Column2" };
            var values = new List<List<string>>
            {
                new List<string> { "Value,Value11", "Value12" },
                new List<string> { "Value21", "Value22" }
            };

            var expectedCsv =
                "\"Column,1\",Column2\n \"Value,Value11\",Value12\n Value21,Value22\n";

            // Act
            string result = "";
            using (var sw = new StringWriter())
            {
                _csvHelper.Write(sw, values, header);
                result = sw.ToString();
            }

            // Assert
            var outcome = SameCsvText(expectedCsv, result);

            Assert.IsFalse(outcome.NumberOfLinesWrong, "The number of lines is not the expected.");
            Assert.IsTrue(outcome.IsSame,
                string.Format("Expected {0} but got {1}.", outcome.ExpectedLine, outcome.ActualLine));
        }

        [Test]
        public void Write_QuotesValuesWithNewLines()
        {
            // Arrange
            var values = new List<List<string>>
            {
                new List<string> { "Value\nValue11", "Value12" },
                new List<string> { "Value21", "Value22" }
            };

            var expectedCsv =
                "\"Value\nValue11\",Value12\n Value21,Value22\n";

            // Act
            string result = "";
            using (var sw = new StringWriter())
            {
                _csvHelper.Write(sw, values);
                result = sw.ToString();
            }

            // Assert
            var outcome = SameCsvText(expectedCsv, result);

            Assert.IsFalse(outcome.NumberOfLinesWrong, "The number of lines is not the expected.");
            Assert.IsTrue(outcome.IsSame,
                string.Format("Expected {0} but got {1}.", outcome.ExpectedLine, outcome.ActualLine));
        }

        [Test]
        public void Write_QuotesValuesWithNewLines_WithHeader()
        {
            // Arrange
            var header = new string[] { "Column\n1", "Column2" };
            var values = new List<List<string>>
            {
                new List<string> { "Value\nValue11", "Value12" },
                new List<string> { "Value21", "Value22" }
            };

            var expectedCsv =
                "\"Column\n1\",Column2\n \"Value\nValue11\",Value12\n Value21,Value22\n";

            // Act
            string result = "";
            using (var sw = new StringWriter())
            {
                _csvHelper.Write(sw, values, header);
                result = sw.ToString();
            }

            // Assert
            var outcome = SameCsvText(expectedCsv, result);

            Assert.IsFalse(outcome.NumberOfLinesWrong, "The number of lines is not the expected.");
            Assert.IsTrue(outcome.IsSame,
                string.Format("Expected {0} but got {1}.", outcome.ExpectedLine, outcome.ActualLine));
        }

        [Test]
        public void Write_DoublesUpQuotesAndThenQuotesTheValue()
        {
            // Arrange
            var values = new List<List<string>>
            {
                new List<string> { "Value\"Value11", "Value12" },
                new List<string> { "Value21", "Value\"Value22" }
            };

            var expectedCsv =
                "\"Value\"\"Value11\",Value12\n Value21,\"Value\"\"Value22\"\n";

            // Act
            string result = "";
            using (var sw = new StringWriter())
            {
                _csvHelper.Write(sw, values);
                result = sw.ToString();
            }

            // Assert
            var outcome = SameCsvText(expectedCsv, result);

            Assert.IsFalse(outcome.NumberOfLinesWrong, "The number of lines is not the expected.");
            Assert.IsTrue(outcome.IsSame,
                string.Format("Expected {0} but got {1}.", outcome.ExpectedLine, outcome.ActualLine));
        }

        [Test]
        public void Write_DoublesUpQuotesAndThenQuotesTheValue_WithHeader()
        {
            // Arrange
            var header = new string[] { "Column\"1", "Column2" };
            var values = new List<List<string>>
            {
                new List<string> { "Value\"Value11", "Value12" },
                new List<string> { "Value21", "Value\"Value22" }
            };

            var expectedCsv =
                "\"Column\"\"1\",Column2\n \"Value\"\"Value11\",Value12\n Value21,\"Value\"\"Value22\"\n";

            // Act
            string result = "";
            using (var sw = new StringWriter())
            {
                _csvHelper.Write(sw, values, header);
                result = sw.ToString();
            }

            // Assert
            var outcome = SameCsvText(expectedCsv, result);

            Assert.IsFalse(outcome.NumberOfLinesWrong, "The number of lines is not the expected.");
            Assert.IsTrue(outcome.IsSame,
                string.Format("Expected {0} but got {1}.", outcome.ExpectedLine, outcome.ActualLine));
        } 

        private class SameCsvTextOutcome
        {
            public bool NumberOfLinesWrong { get; set; }
            public bool IsSame { get; set; }
            public string ExpectedLine { get; set; }
            public string ActualLine { get; set; }
        }

        private SameCsvTextOutcome SameCsvText(string expected, string actual)
        {
            var expectedLines = SplitIntoLinesAndTrim(expected);
            var actualLines = SplitIntoLinesAndTrim(actual);

            if (expectedLines.Count() != expectedLines.Count())
            {
                return new SameCsvTextOutcome { NumberOfLinesWrong = true };
            }

            foreach (var pair in expectedLines.Zip(actualLines, (e, a) => new { Expected = e, Actual = a}))
            {
                if (!pair.Expected.Equals(pair.Actual))
                    return new SameCsvTextOutcome
                    {
                        IsSame = false,
                        ExpectedLine = pair.Expected,
                        ActualLine = pair.Actual
                    };

            }

            return new SameCsvTextOutcome { IsSame = true };
        }

        private List<string> SplitIntoLinesAndTrim(string text)
        {
            var result = new List<string>();
            using (var sr = new StringReader(text))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    result.Add(line.Trim());
                }
            }
            return result;
        }
    }
}
