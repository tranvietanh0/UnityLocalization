#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace The1Studio.UnityLocalizationTool.Editor.Scanning
{
    public static class LocalizationUiScanner
    {
        private static readonly Regex STRING_LITERAL_REGEX = new("\"(?<text>[A-Z][A-Z0-9 _{}.-]{2,})\"", RegexOptions.Compiled);
        private static readonly Regex INVALID_KEY_CHARACTER_REGEX = new("[^a-z0-9_.]+", RegexOptions.Compiled);

        public static IReadOnlyList<LocalizationScanResult> ScanProject()
        {
            var results = new List<LocalizationScanResult>();
            results.AddRange(ScanPrefabs());
            results.AddRange(ScanScreenScripts());
            return results;
        }

        private static IEnumerable<LocalizationScanResult> ScanPrefabs()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith("Assets/Submodules/"))
                {
                    continue;
                }

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                foreach (var text in prefab.GetComponentsInChildren<TMP_Text>(true))
                {
                    var currentText = text.text?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(currentText))
                    {
                        continue;
                    }

                    yield return CreateResult(path, GetHierarchyPath(text.transform), currentText);
                }
            }
        }

        private static IEnumerable<LocalizationScanResult> ScanScreenScripts()
        {
            var root = "Assets/Scripts/Scenes/Screen";
            if (!Directory.Exists(root))
            {
                yield break;
            }

            foreach (var filePath in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                var assetPath = filePath.Replace('\\', '/');
                var content = File.ReadAllText(filePath);
                foreach (Match match in STRING_LITERAL_REGEX.Matches(content))
                {
                    var text = match.Groups["text"].Value.Trim();
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }

                    yield return CreateResult(assetPath, "C# string literal", text);
                }
            }
        }

        private static LocalizationScanResult CreateResult(string assetPath, string hierarchyPath, string currentText)
        {
            var confidence = Classify(currentText, out var reason);
            return new LocalizationScanResult
            {
                AssetPath = assetPath,
                HierarchyPath = hierarchyPath,
                CurrentText = currentText,
                SuggestedKey = SuggestKey(assetPath, currentText),
                Confidence = confidence,
                Reason = reason,
            };
        }

        private static LocalizationScanConfidence Classify(string text, out string reason)
        {
            if (text.All(char.IsDigit))
            {
                reason = "Numeric-only text.";
                return LocalizationScanConfidence.Ignored;
            }

            if (text.Any(char.IsDigit) || text.Contains('{') || text.Contains('}'))
            {
                reason = "Dynamic-looking text requires review.";
                return LocalizationScanConfidence.Review;
            }

            reason = "Static TMP/user-facing candidate.";
            return LocalizationScanConfidence.Safe;
        }

        private static string SuggestKey(string assetPath, string text)
        {
            var fileName = Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
            var prefix = fileName.Replace("screenview", string.Empty).Replace("screen", string.Empty);
            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = "ui";
            }

            var normalized = text.ToLowerInvariant().Replace(' ', '_');
            normalized = INVALID_KEY_CHARACTER_REGEX.Replace(normalized, "_").Trim('_');
            return $"{prefix}.{normalized}";
        }

        private static string GetHierarchyPath(Transform transform)
        {
            var names = new Stack<string>();
            while (transform != null)
            {
                names.Push(transform.name);
                transform = transform.parent;
            }

            return string.Join("/", names);
        }
    }
}
