#nullable enable

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace The1Studio.UnityLocalizationTool.Editor.Validation
{
    public static class LocalizationValidationCli
    {
        public static void Run()
        {
            var issues = LocalizationProjectValidator.ValidateProject();
            foreach (var issue in issues)
            {
                if (issue.Severity == LocalizationValidationSeverity.Critical)
                {
                    Debug.LogError(issue.ToString());
                }
                else
                {
                    Debug.LogWarning(issue.ToString());
                }
            }

            if (issues.Any(issue => issue.Severity == LocalizationValidationSeverity.Critical))
            {
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("Localization validation passed.");
            EditorApplication.Exit(0);
        }
    }
}
