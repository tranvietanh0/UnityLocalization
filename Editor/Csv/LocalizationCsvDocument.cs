#nullable enable

using System.Collections.Generic;

namespace The1Studio.UnityLocalizationTool.Editor.Csv
{
    public sealed class LocalizationCsvDocument
    {
        public IReadOnlyList<string> Headers { get; init; } = new List<string>();
        public IReadOnlyList<string> LocaleColumns { get; init; } = new List<string>();
        public IReadOnlyList<LocalizationCsvRow> Rows { get; init; } = new List<LocalizationCsvRow>();
    }
}
