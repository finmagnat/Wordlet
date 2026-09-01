#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AndroidKeystoreCredentials
{
    private const string KeystorePasswordVariable = "UNITY_KEYSTORE_PASS";
    private const string KeyaliasPasswordVariable = "UNITY_KEYALIAS_PASS";

    static AndroidKeystoreCredentials()
    {
        EditorApplication.delayCall += ApplyOnEditorStartup;
    }

    [MenuItem("Tools/Android/Check Keystore Credentials")]
    public static void CheckCredentials()
    {
        bool hasKeystoreName = !string.IsNullOrEmpty(PlayerSettings.Android.keystoreName);
        bool hasKeyaliasName = !string.IsNullOrEmpty(PlayerSettings.Android.keyaliasName);
        bool hasKeystoreEnvironmentVariable = !string.IsNullOrEmpty(GetUserEnvironmentVariable(KeystorePasswordVariable));
        bool hasKeyaliasEnvironmentVariable = !string.IsNullOrEmpty(GetUserEnvironmentVariable(KeyaliasPasswordVariable));
        bool hasKeystorePassword = !string.IsNullOrEmpty(PlayerSettings.Android.keystorePass);
        bool hasKeyaliasPassword = !string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass);

        string status =
            "[Android Keystore] Credentials status:\n" +
            $"  Keystore name: {ToStatus(hasKeystoreName)}\n" +
            $"  Key alias name: {ToStatus(hasKeyaliasName)}\n" +
            $"  {KeystorePasswordVariable}: {ToStatus(hasKeystoreEnvironmentVariable)}\n" +
            $"  {KeyaliasPasswordVariable}: {ToStatus(hasKeyaliasEnvironmentVariable)}\n" +
            $"  Keystore password in PlayerSettings: {ToStatus(hasKeystorePassword)}\n" +
            $"  Key alias password in PlayerSettings: {ToStatus(hasKeyaliasPassword)}";

        bool allCredentialsAreReady =
            hasKeystoreName &&
            hasKeyaliasName &&
            hasKeystoreEnvironmentVariable &&
            hasKeyaliasEnvironmentVariable &&
            hasKeystorePassword &&
            hasKeyaliasPassword;

        if (allCredentialsAreReady)
            Debug.Log(status);
        else
            Debug.LogWarning(status);
    }

    [MenuItem("Tools/Android/Reload Keystore Credentials")]
    public static void ReloadCredentials()
    {
        int appliedPasswordCount = ApplyCredentialsFromUserEnvironment();
        Debug.Log($"[Android Keystore] Reload complete. Applied {appliedPasswordCount}/2 passwords from user environment variables.");
    }

    private static void ApplyOnEditorStartup()
    {
        EditorApplication.delayCall -= ApplyOnEditorStartup;
        ApplyCredentialsFromUserEnvironment();
    }

    private static int ApplyCredentialsFromUserEnvironment()
    {
        int appliedPasswordCount = 0;

        string keystorePassword = GetUserEnvironmentVariable(KeystorePasswordVariable);
        if (string.IsNullOrEmpty(keystorePassword))
        {
            Debug.LogWarning($"[Android Keystore] User environment variable {KeystorePasswordVariable} is missing or empty. The current keystore password was not changed.");
        }
        else
        {
            PlayerSettings.Android.keystorePass = keystorePassword;
            appliedPasswordCount++;
        }

        string keyaliasPassword = GetUserEnvironmentVariable(KeyaliasPasswordVariable);
        if (string.IsNullOrEmpty(keyaliasPassword))
        {
            Debug.LogWarning($"[Android Keystore] User environment variable {KeyaliasPasswordVariable} is missing or empty. The current key alias password was not changed.");
        }
        else
        {
            PlayerSettings.Android.keyaliasPass = keyaliasPassword;
            appliedPasswordCount++;
        }

        return appliedPasswordCount;
    }

    private static string GetUserEnvironmentVariable(string variableName)
    {
#if UNITY_EDITOR_WIN
        return Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User);
#else
        return Environment.GetEnvironmentVariable(variableName);
#endif
    }

    private static string ToStatus(bool isPresent)
    {
        return isPresent ? "OK" : "MISSING";
    }
}
#endif
