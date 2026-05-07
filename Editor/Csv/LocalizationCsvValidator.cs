#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using The1Studio.UnityLocalizationTool.Editor.Validation;

namespace The1Studio.UnityLocalizationTool.Editor.Csv
{
    public static class LocalizationCsvValidator
    {
        private static readonly HashSet<string> REQUIRED_COLUMNS = new(StringComparer.OrdinalIgnoreCase)
        {
            "table",
            "key",
            "source",
            "en",
        };

        private static readonly HashSet<string> ALLOWED_STATUSES = new(StringComparer.OrdinalIgnoreCase)
        {
            string.Empty,
            "new",
            "approved",
            "deprecated",
            "ignore",
        };

        public static IReadOnlyList<LocalizationValidationIssue> Validate(LocalizationCsvDocument document)
        {
            var issues = new List<LocalizationValidationIssue>();
            ValidateHeaders(document, issues);
            ValidateRows(document, issues);
            return issues;
        }

        private static void ValidateHeaders(LocalizationCsvDocument document, List<LocalizationValidationIssue> issues)
        {
            foreach (var column in REQUIRED_COLUMNS)
            {
                if (document.Headers.Any(header => string.Equals(header, column, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                issues.Add(new LocalizationValidationIssue
                {
                    Severity = LocalizationValidationSeverity.Critical,
                    Code = "csv.missing_column",
                    Message = $"Missing required column '{column}'.",
                });
            }
        }

        private static void ValidateRows(LocalizationCsvDocument document, List<LocalizationValidationIssue> issues)
        {
            var identities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in document.Rows)
            {
                if (string.IsNullOrWhiteSpace(row.Table))
                {
                    issues.Add(CreateRowIssue(row, "csv.missing_table", "Table is required."));
                }

                if (string.IsNullOrWhiteSpace(row.Key))
                {
                    issues.Add(CreateRowIssue(row, "csv.missing_key", "Key is required."));
                }

                if (!string.IsNullOrWhiteSpace(row.Table) && !string.IsNullOrWhiteSpace(row.Key) && !identities.Add(row.Identity))
                {
                    issues.Add(CreateRowIssue(row, "csv.duplicate_key", $"Duplicate table/key '{row.Identity}'."));
                }

                if (!ALLOWED_STATUSES.Contains(row.Status))
                {
                    issues.Add(CreateRowIssue(row, "csv.invalid_status", $"Invalid status '{row.Status}'."));
                }

                if (string.Equals(row.Status, "ignore", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (var localeColumn in document.LocaleColumns)
                {
                    if (string.IsNullOrWhiteSpace(row.LocaleValues.GetValueOrDefault(localeColumn, string.Empty)))
                    {
                        issues.Add(CreateRowIssue(row, "csv.missing_locale_value", $"Missing value for locale '{localeColumn}'."));
                    }
                }
            }
        }

        private static LocalizationValidationIssue CreateRowIssue(LocalizationCsvRow row, string code, string message)
        {
            return new LocalizationValidationIssue
            {
                Severity = LocalizationValidationSeverity.Critical,
                Code = code,
                Message = message,
                AssetPath = row.Path,
                Key = row.Identity,
            };
        }
    }
}
