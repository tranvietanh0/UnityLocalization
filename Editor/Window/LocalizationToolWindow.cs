#nullable enable

using System.Collections.Generic;
using System.Linq;
using The1Studio.UnityLocalizationTool.Editor.Csv;
using The1Studio.UnityLocalizationTool.Editor.Scanning;
using The1Studio.UnityLocalizationTool.Editor.Validation;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace The1Studio.UnityLocalizationTool.Editor.Window
{
    public sealed class LocalizationToolWindow : EditorWindow
    {
        private readonly List<LocalizationValidationIssue> issues = new();
        private readonly List<LocalizationScanResult> scanResults = new();
        private Vector2 scrollPosition;
        private int tabIndex;
        private string csvPath = LocalizationToolPaths.CsvPath;

        [MenuItem("Tools/The1Studio/Localization Tool")]
        public static void Open()
        {
            GetWindow<LocalizationToolWindow>("Localization Tool");
        }

        private void OnGUI()
        {
            this.tabIndex = GUILayout.Toolbar(this.tabIndex, new[] { "Setup", "CSV Import", "Scan UI", "Validate" });
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            switch (this.tabIndex)
            {
                case 0:
                    this.DrawSetupTab();
                    break;
                case 1:
                    this.DrawCsvTab();
                    break;
                case 2:
                    this.DrawScanTab();
                    break;
                case 3:
                    this.DrawValidateTab();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSetupTab()
        {
            EditorGUILayout.LabelField("Localization Settings", LocalizationSetupUtility.HasSettings ? "Ready" : "Missing");
            EditorGUILayout.LabelField("Locales", string.Join(", ", LocalizationEditorSettings.GetLocales().Select(locale => locale.Identifier.Code)));
            EditorGUILayout.LabelField("Default CSV", LocalizationToolPaths.CsvPath);

            if (GUILayout.Button("Create/Repair Localization Setup"))
            {
                LocalizationSetupUtility.CreateOrRepairBaseline();
                this.ShowNotification(new GUIContent("Localization setup repaired."));
            }
        }

        private void DrawCsvTab()
        {
            EditorGUILayout.BeginHorizontal();
            this.csvPath = EditorGUILayout.TextField("CSV Path", this.csvPath);
            if (GUILayout.Button("Pick", GUILayout.Width(80)))
            {
                var picked = EditorUtility.OpenFilePanel("Localization CSV", Application.dataPath, "csv");
                if (!string.IsNullOrWhiteSpace(picked))
                {
                    this.csvPath = FileUtil.GetProjectRelativePath(picked);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Validate CSV"))
            {
                var document = LocalizationCsvParser.ParseFile(this.csvPath);
                this.issues.Clear();
                this.issues.AddRange(LocalizationCsvValidator.Validate(document));
            }

            if (GUILayout.Button("Import CSV to String Tables"))
            {
                this.issues.Clear();
                this.issues.AddRange(LocalizationCsvImporter.Import(this.csvPath));
                if (this.issues.All(issue => issue.Severity != LocalizationValidationSeverity.Critical))
                {
                    this.ShowNotification(new GUIContent("CSV imported."));
                }
            }

            this.DrawIssues();
        }

        private void DrawScanTab()
        {
            if (GUILayout.Button("Scan Project UI"))
            {
                this.scanResults.Clear();
                this.scanResults.AddRange(LocalizationUiScanner.ScanProject());
            }

            EditorGUILayout.LabelField("Results", this.scanResults.Count.ToString());
            foreach (var result in this.scanResults)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(result.CurrentText, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Confidence", result.Confidence.ToString());
                EditorGUILayout.LabelField("Suggested Key", result.SuggestedKey);
                EditorGUILayout.LabelField("Asset", result.AssetPath);
                EditorGUILayout.LabelField("Path", result.HierarchyPath);
                EditorGUILayout.LabelField("Reason", result.Reason);
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawValidateTab()
        {
            if (GUILayout.Button("Run Validation"))
            {
                this.issues.Clear();
                this.issues.AddRange(LocalizationProjectValidator.ValidateProject(this.csvPath));
            }

            this.DrawIssues();
        }

        private void DrawIssues()
        {
            EditorGUILayout.LabelField("Issues", this.issues.Count.ToString());
            foreach (var issue in this.issues)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"{issue.Severity} — {issue.Code}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(issue.Message, EditorStyles.wordWrappedLabel);
                if (!string.IsNullOrWhiteSpace(issue.AssetPath))
                {
                    EditorGUILayout.LabelField("Asset", issue.AssetPath);
                }

                if (!string.IsNullOrWhiteSpace(issue.Key))
                {
                    EditorGUILayout.LabelField("Key", issue.Key);
                }

                EditorGUILayout.EndVertical();
            }
        }
    }
}
