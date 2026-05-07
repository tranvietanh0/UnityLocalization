#nullable enable

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace The1Studio.UnityLocalizationTool.Editor
{
    public static class LocalizationSetupUtility
    {
        public static bool HasSettings => LocalizationEditorSettings.ActiveLocalizationSettings != null;

        public static void CreateOrRepairBaseline()
        {
            EnsureFolders();
            EnsureSettings();
            EnsureLocale("en");
            EnsureLocale("vi");
            EnsureStringTableCollection("UI");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static Locale EnsureLocale(string localeCode)
        {
            var existingLocale = LocalizationEditorSettings.GetLocales()
                .FirstOrDefault(locale => locale.Identifier.Code == localeCode);
            if (existingLocale != null)
            {
                return existingLocale;
            }

            EnsureFolders();
            var locale = Locale.CreateLocale(new LocaleIdentifier(localeCode));
            var assetPath = $"{LocalizationToolPaths.LocaleFolder}/{localeCode}.asset";
            AssetDatabase.CreateAsset(locale, AssetDatabase.GenerateUniqueAssetPath(assetPath));
            LocalizationEditorSettings.AddLocale(locale);
            EditorUtility.SetDirty(locale);
            return locale;
        }

        public static void EnsureStringTableCollection(string tableName)
        {
            if (LocalizationEditorSettings.GetStringTableCollection(tableName) != null)
            {
                return;
            }

            EnsureFolders();
            var collection = LocalizationEditorSettings.CreateStringTableCollection(tableName, LocalizationToolPaths.TableFolder);
            EditorUtility.SetDirty(collection.SharedData);
        }

        private static void EnsureSettings()
        {
            if (LocalizationEditorSettings.ActiveLocalizationSettings != null)
            {
                return;
            }

            var settings = ScriptableObject.CreateInstance<LocalizationSettings>();
            AssetDatabase.CreateAsset(settings, LocalizationToolPaths.SettingsPath);
            LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            EditorUtility.SetDirty(settings);
        }

        private static void EnsureFolders()
        {
            CreateFolder("Assets", "Localization");
            CreateFolder(LocalizationToolPaths.LocalizationRoot, "Locales");
            CreateFolder(LocalizationToolPaths.LocalizationRoot, "StringTables");
        }

        private static void CreateFolder(string parent, string folder)
        {
            var path = Path.Combine(parent, folder).Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
