#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using The1Studio.UnityLocalizationTool.Editor.Csv;
using The1Studio.UnityLocalizationTool.Editor.Scanning;
using UnityEditor.Localization;

namespace The1Studio.UnityLocalizationTool.Editor.Validation
{
    public static class LocalizationProjectValidator
    {
        public static IReadOnlyList<LocalizationValidationIssue> ValidateProject(string csvPath = LocalizationToolPaths.CsvPath)
        {
            var issues = new List<LocalizationValidationIssue>();
            if (!LocalizationSetupUtility.HasSettings)
            {
                issues.Add(new LocalizationValidationIssue
                {
                    Severity = LocalizationValidationSeverity.Critical,
                    Code = "setup.missing_settings",
                    Message = "Localization Settings asset is missing.",
                });
            }

            if (!File.Exists(csvPath))
            {
                issues.Add(new LocalizationValidationIssue
                {
                    Severity = LocalizationValidationSeverity.Critical,
                    Code = "csv.missing_file",
                    Message = $"CSV file not found at '{csvPath}'.",
                    AssetPath = csvPath,
                });
                return issues;
            }

            var document = LocalizationCsvParser.ParseFile(csvPath);
            issues.AddRange(LocalizationCsvValidator.Validate(document));
            issues.AddRange(ValidateTables(document));
            issues.AddRange(ValidateHardcodedText());
            return issues;
        }

        private static IEnumerable<LocalizationValidationIssue> ValidateTables(LocalizationCsvDocument document)
        {
            foreach (var tableGroup in document.Rows.GroupBy(row => row.Table))
            {
                var collection = LocalizationEditorSettings.GetStringTableCollection(tableGroup.Key);
                if (collection == null)
                {
                    yield return new LocalizationValidationIssue
                    {
                        Severity = LocalizationValidationSeverity.Critical,
                        Code = "table.missing_collection",
                        Message = $"String Table Collection '{tableGroup.Key}' is missing.",
                        Key = tableGroup.Key,
                    };
                    continue;
                }

                foreach (var localeColumn in document.LocaleColumns)
                {
                    var locale = LocalizationEditorSettings.GetLocales()
                        .FirstOrDefault(locale => locale.Identifier.Code == localeColumn);
                    if (locale == null)
                    {
                        yield return new LocalizationValidationIssue
                        {
                            Severity = LocalizationValidationSeverity.Critical,
                            Code = "locale.missing_asset",
                            Message = $"Locale asset '{localeColumn}' is missing.",
                            Key = localeColumn,
                        };
                        continue;
                    }

                    var table = collection.GetTable(locale.Identifier);
                    if (table == null)
                    {
                        yield return new LocalizationValidationIssue
                        {
                            Severity = LocalizationValidationSeverity.Critical,
                            Code = "table.missing_locale_table",
                            Message = $"Table '{tableGroup.Key}' has no locale table for '{localeColumn}'.",
                            Key = tableGroup.Key,
                        };
                    }
                }
            }
        }

        private static IEnumerable<LocalizationValidationIssue> ValidateHardcodedText()
        {
            foreach (var result in LocalizationUiScanner.ScanProject())
            {
                if (result.Confidence == LocalizationScanConfidence.Ignored)
                {
                    continue;
                }

                yield return new LocalizationValidationIssue
                {
                    Severity = result.Confidence == LocalizationScanConfidence.Safe
                        ? LocalizationValidationSeverity.Critical
                        : LocalizationValidationSeverity.Warning,
                    Code = "scan.hardcoded_text",
                    Message = $"Hardcoded text '{result.CurrentText}' requires localization review. Suggested key: {result.SuggestedKey}.",
                    AssetPath = result.AssetPath,
                    Key = result.SuggestedKey,
                };
            }
        }
    }
}
