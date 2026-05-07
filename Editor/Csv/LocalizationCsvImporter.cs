#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using The1Studio.UnityLocalizationTool.Editor.Validation;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace The1Studio.UnityLocalizationTool.Editor.Csv
{
    public static class LocalizationCsvImporter
    {
        public static IReadOnlyList<LocalizationValidationIssue> Import(string csvAssetPath)
        {
            var absolutePath = Path.GetFullPath(csvAssetPath);
            var document = LocalizationCsvParser.ParseFile(absolutePath);
            var issues = LocalizationCsvValidator.Validate(document);
            if (issues.Any(issue => issue.Severity == LocalizationValidationSeverity.Critical))
            {
                return issues;
            }

            LocalizationSetupUtility.CreateOrRepairBaseline();
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var localeColumn in document.LocaleColumns)
                {
                    LocalizationSetupUtility.EnsureLocale(localeColumn);
                }

                foreach (var tableGroup in document.Rows
                             .Where(row => !string.Equals(row.Status, "ignore", System.StringComparison.OrdinalIgnoreCase))
                             .GroupBy(row => row.Table))
                {
                    ImportTable(tableGroup.Key, tableGroup.ToList(), document.LocaleColumns);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return issues;
        }

        private static void ImportTable(string tableName, IReadOnlyList<LocalizationCsvRow> rows, IReadOnlyList<string> localeColumns)
        {
            LocalizationSetupUtility.EnsureStringTableCollection(tableName);
            var collection = LocalizationEditorSettings.GetStringTableCollection(tableName);
            if (collection == null)
            {
                throw new InvalidDataException($"Failed to create String Table Collection '{tableName}'.");
            }

            foreach (var localeColumn in localeColumns)
            {
                var locale = LocalizationSetupUtility.EnsureLocale(localeColumn);
                var table = collection.GetTable(locale.Identifier) as StringTable;
                table ??= collection.AddNewTable(locale.Identifier) as StringTable;
                if (table == null)
                {
                    throw new InvalidDataException($"Failed to create String Table '{tableName}' for locale '{localeColumn}'.");
                }

                foreach (var row in rows)
                {
                    var value = row.LocaleValues.GetValueOrDefault(localeColumn, string.Empty);
                    var entry = table.GetEntry(row.Key) ?? table.AddEntry(row.Key, value);
                    entry.Value = value;
                    entry.IsSmart = row.IsSmart;
                }

                EditorUtility.SetDirty(table);
            }

            EditorUtility.SetDirty(collection.SharedData);
        }
    }
}
