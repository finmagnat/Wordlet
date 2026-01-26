#if UNITY_EDITOR
using System;
using System.IO;
using Core.Build;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildInfoBuildHook : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        // Путь к SO (ты можешь поменять)
        const string assetPath = "Assets/Resources/BuildInfo.asset";

        var info = AssetDatabase.LoadAssetAtPath<BuildInfoSO>(assetPath);
        if (info == null)
            throw new Exception($"BuildInfoSO not found at path: {assetPath}. Create it via CreateAssetMenu.");

        var utc = DateTime.UtcNow;
        var local = DateTime.Now;

        info.buildUtc = utc.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
        info.buildLocal = local.ToString("yyyy-MM-dd HH:mm:ss");
        info.unityVersion = Application.unityVersion;
        info.platform = report.summary.platform.ToString();
        info.versionName = PlayerSettings.bundleVersion;

        // “конфигурация” — на твой вкус
        info.configuration =
            EditorUserBuildSettings.development ? "Development" : "Release";

#if UNITY_ANDROID
        info.androidVersionCode = PlayerSettings.Android.bundleVersionCode;
#else
        info.androidVersionCode = 0;
#endif

        EditorUtility.SetDirty(info);
        AssetDatabase.SaveAssets();

        Debug.Log($"[BuildInfo] Updated: {info.buildUtc} ({info.configuration}) {info.platform}");
    }
}
#endif