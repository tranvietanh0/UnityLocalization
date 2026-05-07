#nullable enable

using System.Collections.Generic;

namespace The1Studio.UnityLocalizationTool.Editor.Csv
{
    public sealed class LocalizationCsvRow
    {
        public string Table { get; init; } = string.Empty;
        public string Key { get; init; } = string.Empty;
        public string Source { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Path { get; init; } = string.Empty;
        public string ComponentGuid { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public bool IsSmart { get; init; }
        public string Notes { get; init; } = string.Empty;
        public Dictionary<string, string> LocaleValues { get; init; } = new();

        public string Identity => $"{this.Table}/{this.Key}";
    }
}
