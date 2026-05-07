#nullable enable

namespace The1Studio.UnityLocalizationTool.Editor.Scanning
{
    public sealed class LocalizationScanResult
    {
        public string AssetPath { get; init; } = string.Empty;
        public string HierarchyPath { get; init; } = string.Empty;
        public string CurrentText { get; init; } = string.Empty;
        public string SuggestedKey { get; init; } = string.Empty;
        public string Reason { get; init; } = string.Empty;
        public LocalizationScanConfidence Confidence { get; init; }
    }
}
