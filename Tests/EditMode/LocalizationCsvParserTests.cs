#nullable enable

using System.Linq;
using NUnit.Framework;
using The1Studio.UnityLocalizationTool.Editor.Csv;
using The1Studio.UnityLocalizationTool.Editor.Validation;

namespace The1Studio.UnityLocalizationTool.Tests
{
    public sealed class LocalizationCsvParserTests
    {
        [Test]
        public void Should_parseQuotedFields_when_csvContainsCommasQuotesAndBraces()
        {
            const string csv = "table,key,source,en,vi,status,smart\nUI,home.title,\"Hello, \"\"Player\"\"\",\"Hello, {0}\",\"Xin chào, {0}\",approved,true\n";

            var document = LocalizationCsvParser.Parse(csv);
            var row = document.Rows.Single();

            Assert.AreEqual("UI", row.Table);
            Assert.AreEqual("home.title", row.Key);
            Assert.AreEqual("Hello, \"Player\"", row.Source);
            Assert.AreEqual("Hello, {0}", row.LocaleValues["en"]);
            Assert.IsTrue(row.IsSmart);
        }

        [Test]
        public void Should_returnCriticalIssue_when_duplicateTableKeyExists()
        {
            const string csv = "table,key,source,en,status\nUI,home.title,Title,Title,approved\nUI,home.title,Title,Title,approved\n";

            var document = LocalizationCsvParser.Parse(csv);
            var issues = LocalizationCsvValidator.Validate(document);

            Assert.IsTrue(issues.Any(issue => issue.Code == "csv.duplicate_key" && issue.Severity == LocalizationValidationSeverity.Critical));
        }

        [Test]
        public void Should_returnCriticalIssue_when_requiredLocaleValueMissing()
        {
            const string csv = "table,key,source,en,status\nUI,home.title,Title,,approved\n";

            var document = LocalizationCsvParser.Parse(csv);
            var issues = LocalizationCsvValidator.Validate(document);

            Assert.IsTrue(issues.Any(issue => issue.Code == "csv.missing_locale_value" && issue.Severity == LocalizationValidationSeverity.Critical));
        }
    }
}
