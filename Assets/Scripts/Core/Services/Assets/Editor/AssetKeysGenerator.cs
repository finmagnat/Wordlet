#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

public static class AssetKeysGenerator
{
    private const string EnumPath = "Assets/Scripts/Core/Generated/AssetKey.cs";
    private const string DatabasePath = "Assets/Resources/Config/AssetKeysDatabase.asset";

    [MenuItem("Tools/Generate/AssetKeys")]
    public static void Generate()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressables not configured!");
            return;
        }

        // Prepare database
        var db = AssetDatabase.LoadAssetAtPath<AssetKeysDatabase>(DatabasePath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<AssetKeysDatabase>();
            Directory.CreateDirectory("Assets/Resources/Config");
            AssetDatabase.CreateAsset(db, DatabasePath);
        }

        db.Entries.Clear();

        var enums = new System.Collections.Generic.List<string>();

        foreach (var group in settings.groups)
        {
            if (group == null || group.ReadOnly) continue;

            foreach (var entry in group.entries)
            {
                string key = entry.address;
                string id = SanitizeName(entry.address);

                db.Entries.Add(new AssetKeysDatabase.Entry()
                {
                    Id = id,
                    AddressKey = key
                });

                enums.Add(id);
            }
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        GenerateEnum(enums);

        Debug.Log("✔ AssetKeys regenerated!");
    }

    private static void GenerateEnum(System.Collections.Generic.List<string> keys)
    {
        Directory.CreateDirectory("Assets/Scripts/Core/Generated");

        using (var writer = new StreamWriter(EnumPath))
        {
            writer.WriteLine("namespace Core.Generated");
            writer.WriteLine("{");
            writer.WriteLine("    public enum AssetKey");
            writer.WriteLine("    {");

            foreach (var key in keys)
                writer.WriteLine($"        {key},");

            writer.WriteLine("    }");
            writer.WriteLine("}");
        }

        AssetDatabase.Refresh();
    }

    private static string SanitizeName(string input)
    {
        input = Path.GetFileNameWithoutExtension(input);
        input = input.Replace(" ", "_")
                     .Replace("-", "_")
                     .Replace(".", "_");
        return input;
    }
}
#endif
