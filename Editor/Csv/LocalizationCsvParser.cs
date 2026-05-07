#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace The1Studio.UnityLocalizationTool.Editor.Csv
{
    public static class LocalizationCsvParser
    {
        private static readonly HashSet<string> RESERVED_COLUMNS = new(StringComparer.OrdinalIgnoreCase)
        {
            "table",
            "key",
            "source",
            "description",
            "path",
            "componentGuid",
            "status",
            "smart",
            "notes",
        };

        public static LocalizationCsvDocument ParseFile(string path)
        {
            return Parse(File.ReadAllText(path, Encoding.UTF8));
        }

        public static LocalizationCsvDocument Parse(string content)
        {
            var records = ReadRecords(content);
            if (records.Count == 0)
            {
                return new LocalizationCsvDocument();
            }

            var headers = records[0].Select(header => header.Trim()).ToList();
            var localeColumns = headers.Where(header => !RESERVED_COLUMNS.Contains(header)).ToList();
            var rows = new List<LocalizationCsvRow>();

            for (var index = 1; index < records.Count; index++)
            {
                var record = records[index];
                if (record.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (var column = 0; column < headers.Count; column++)
                {
                    values[headers[column]] = column < record.Count ? record[column].Trim() : string.Empty;
                }

                var localeValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var localeColumn in localeColumns)
                {
                    localeValues[localeColumn] = values.GetValueOrDefault(localeColumn, string.Empty);
                }

                rows.Add(new LocalizationCsvRow
                {
                    Table = values.GetValueOrDefault("table", string.Empty),
                    Key = values.GetValueOrDefault("key", string.Empty),
                    Source = values.GetValueOrDefault("source", string.Empty),
                    Description = values.GetValueOrDefault("description", string.Empty),
                    Path = values.GetValueOrDefault("path", string.Empty),
                    ComponentGuid = values.GetValueOrDefault("componentGuid", string.Empty),
                    Status = values.GetValueOrDefault("status", string.Empty),
                    IsSmart = string.Equals(values.GetValueOrDefault("smart", string.Empty), "true", StringComparison.OrdinalIgnoreCase),
                    Notes = values.GetValueOrDefault("notes", string.Empty),
                    LocaleValues = localeValues,
                });
            }

            return new LocalizationCsvDocument
            {
                Headers = headers,
                LocaleColumns = localeColumns,
                Rows = rows,
            };
        }

        private static List<List<string>> ReadRecords(string content)
        {
            var records = new List<List<string>>();
            var record = new List<string>();
            var field = new StringBuilder();
            var isQuoted = false;

            for (var index = 0; index < content.Length; index++)
            {
                var character = content[index];
                if (isQuoted)
                {
                    if (character == '"')
                    {
                        if (index + 1 < content.Length && content[index + 1] == '"')
                        {
                            field.Append('"');
                            index++;
                        }
                        else
                        {
                            isQuoted = false;
                        }
                    }
                    else
                    {
                        field.Append(character);
                    }

                    continue;
                }

                switch (character)
                {
                    case '"' when field.Length == 0:
                        isQuoted = true;
                        break;
                    case ',':
                        record.Add(field.ToString());
                        field.Clear();
                        break;
                    case '\r':
                        break;
                    case '\n':
                        record.Add(field.ToString());
                        field.Clear();
                        records.Add(record);
                        record = new List<string>();
                        break;
                    default:
                        field.Append(character);
                        break;
                }
            }

            if (field.Length > 0 || record.Count > 0)
            {
                record.Add(field.ToString());
                records.Add(record);
            }

            return records;
        }
    }
}
