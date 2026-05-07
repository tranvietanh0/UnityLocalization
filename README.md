# UnityLocalization

Unity Localization tooling for CSV-first text localization.

## Scope

- CSV schema validation.
- CSV import into Unity Localization String Table Collections.
- Locale/settings bootstrap for `en` and `vi`.
- Project UI scan for hardcoded `TMP_Text` and screen-code string literals.
- Validation CLI for batchmode checks.
- Runtime `LocalizedTmpText` helper for TMP labels created from code.

## Install in a Unity project

Add the local package to `Packages/manifest.json`:

```json
"com.the1studio.unity-localization-tool": "file:UnityLocalization"
```

The consuming project also needs `com.unity.localization`.

## Menu

`Tools/The1Studio/Localization Tool`

## Runtime helper

Use `The1Studio.UnityLocalizationTool.Runtime.LocalizedTmpText` for TMP labels created from code. It subscribes to `LocalizedString.StringChanged`, so locale switches update text asynchronously.

## Default CSV path

`Assets/LocalizationSource/localization.csv`

Required columns:

```csv
table,key,source,en
```

Recommended columns:

```csv
table,key,source,en,vi,description,path,componentGuid,status,smart,notes
```

## Batch validation

```bash
Unity -batchmode -projectPath "<project-path>" -executeMethod The1Studio.UnityLocalizationTool.Editor.Validation.LocalizationValidationCli.Run -quit
```
