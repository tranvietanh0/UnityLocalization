#nullable enable

namespace The1Studio.UnityLocalizationTool.Editor.Validation
{
    public sealed class LocalizationValidationIssue
    {
        public LocalizationValidationSeverity Severity { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string AssetPath { get; init; } = string.Empty;
        public string Key { get; init; } = string.Empty;

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(this.AssetPath)
                ? $"[{this.Severity}] {this.Code}: {this.Message}"
                : $"[{this.Severity}] {this.Code}: {this.Message} ({this.AssetPath})";
        }
    }
}
