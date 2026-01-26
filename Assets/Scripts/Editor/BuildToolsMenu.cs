#if UNITY_EDITOR
using System;
using Core.Build;
using UnityEditor;
using UnityEngine;

public static class BuildToolsMenu
{
    private const string BuildInfoAssetPath = "Assets/Resources/BuildInfo.asset";

    [MenuItem("Tools/Build/Increment Android Version Code")]
    public static void IncrementAndroidVersionCode()
    {
#if !UNITY_ANDROID
        EditorUtility.DisplayDialog("Build Tools", "Switch platform to Android first.", "OK");
        return;
#else
        int oldCode = PlayerSettings.Android.bundleVersionCode;
        int newCode = oldCode + 1;

        PlayerSettings.Android.bundleVersionCode = newCode;

        // Опционально: сразу обновим BuildInfoSO (чтобы в инспекторе было видно)
        TryUpdateBuildInfoAsset(versionCode: newCode);

        Debug.Log($"[BuildTools] Android versionCode: {oldCode} -> {newCode}");
#endif
    }

    [MenuItem("Tools/Build/Set Android Version Code...")]
    public static void SetAndroidVersionCode()
    {
#if !UNITY_ANDROID
        EditorUtility.DisplayDialog("Build Tools", "Switch platform to Android first.", "OK");
        return;
#else
        string input = EditorUtility.DisplayDialogComplex(
            "Set Version Code",
            "This will open the Console with instructions.\n(Unity doesn't have a built-in numeric prompt.)",
            "OK", "Cancel", ""
        ) == 0 ? "OK" : "Cancel";

        if (input != "OK") return;

        Debug.Log("[BuildTools] Use menu 'Tools/Build/Set Android Version Code From Clipboard' after copying a number to clipboard.");
#endif
    }

    [MenuItem("Tools/Build/Set Android Version Code From Clipboard")]
    public static void SetAndroidVersionCodeFromClipboard()
    {
#if !UNITY_ANDROID
        EditorUtility.DisplayDialog("Build Tools", "Switch platform to Android first.", "OK");
        return;
#else
        string text = EditorGUIUtility.systemCopyBuffer?.Trim();
        if (!int.TryParse(text, out int code) || code <= 0)
        {
            EditorUtility.DisplayDialog("Build Tools", $"Clipboard does not contain a valid positive integer.\nClipboard: '{text}'", "OK");
            return;
        }

        int oldCode = PlayerSettings.Android.bundleVersionCode;
        PlayerSettings.Android.bundleVersionCode = code;

        TryUpdateBuildInfoAsset(versionCode: code);

        Debug.Log($"[BuildTools] Android versionCode: {oldCode} -> {code}");
#endif
    }

    private static void TryUpdateBuildInfoAsset(int versionCode)
    {
        var info = AssetDatabase.LoadAssetAtPath<BuildInfoSO>(BuildInfoAssetPath);
        if (info == null) return;

        info.androidVersionCode = versionCode;
        info.buildUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
        info.buildLocal = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        EditorUtility.SetDirty(info);
        AssetDatabase.SaveAssets();
    }
}
#endif
