using System.Collections.Generic;
using System.IO;
using Core.DataDictionary;
using UnityEditor;
using UnityEngine;

public class DictionaryImporter : EditorWindow
{
    private TextAsset _naninovelJson;
    private string _outputFileName = "dict_xx.txt";
    private LanguageDictionaryConfig _targetConfig;

    [MenuItem("Tools/Import/Dictionary From Naninovel JSON")]
    public static void ShowWindow()
    {
        GetWindow<DictionaryImporter>("Dictionary Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("JSON → dict_xx.txt Importer", EditorStyles.boldLabel);

        _naninovelJson = (TextAsset)EditorGUILayout.ObjectField("Naninovel JSON", _naninovelJson, typeof(TextAsset), false);
        _targetConfig = (LanguageDictionaryConfig)EditorGUILayout.ObjectField("Target Config", _targetConfig, typeof(LanguageDictionaryConfig), false);

        _outputFileName = EditorGUILayout.TextField("Output File Name", _outputFileName);

        if (GUILayout.Button("Import"))
        {
            if (_naninovelJson == null)
            {
                Debug.LogError("No JSON file assigned");
                return;
            }

            Import();
        }
    }

    private void Import()
    {
        var json = JsonUtility.FromJson<NaninovelLanguagesRoot>(_naninovelJson.text);

        if (json.languages == null || json.languages.Length == 0)
        {
            Debug.LogError("No languages found in JSON.");
            return;
        }

        // Берём первый язык (ru, en, uk — выбирай нужный)
        var lang = json.languages[2];

        string alphabet = lang.Library.dictionaryWords.Alphabet;
        List<string> words = lang.Library.dictionaryWords.Words;

        // Создаём txt
        string outputDir = "Assets/Addressables/Text/Dictionaries/";
        Directory.CreateDirectory(outputDir);

        string outputPath = Path.Combine(outputDir, _outputFileName);

        File.WriteAllLines(outputPath, words);
        AssetDatabase.Refresh();

        // Подгружаем TextAsset
        /*var txtAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(outputPath);

        if (_targetConfig != null)
        {
            _targetConfig.SetAlphabet(alphabet);
            _targetConfig.SetDictionaryFile(txtAsset);

            EditorUtility.SetDirty(_targetConfig);
            AssetDatabase.SaveAssets();
        }*/

        Debug.Log($"✔ Dictionary imported. Words: {words.Count}. File: {outputPath}");
    }

    // -------- JSON модели --------
    [System.Serializable]
    public class NaninovelLanguagesRoot
    {
        public string PathPrefix;
        public NaninovelLangEntry[] languages;
    }

    [System.Serializable]
    public class NaninovelLangEntry
    {
        public string lang;
        public string fullName;
        public NaninovelLibrary Library;
    }

    [System.Serializable]
    public class NaninovelLibrary
    {
        public NaninovelWords dictionaryWords;
    }

    [System.Serializable]
    public class NaninovelWords
    {
        public string Alphabet;
        public List<string> Words;
    }
}
