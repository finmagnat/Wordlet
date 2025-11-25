using UnityEngine;

namespace Core.Dictionary
{
    [CreateAssetMenu(menuName = "Balda/Dictionary/Language Config", fileName = "LanguageDictionaryConfig")]
    public class LanguageDictionaryConfig : ScriptableObject
    {
        [Header("Language Info")]
        public string languageCode;       // "ru", "en", "uk"
        public string languageName;       // Русский, English, Українська

        [Header("Alphabet")]
        [TextArea] 
        public string alphabet;

        [Header("Addressables Key for Dictionary File")]
        public string dictionaryAddressKey; // пример: "dict_ru"
    }
}
/*
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Language Dictionary", fileName = "LanguageDictionaryConfig")]
public class LanguageDictionaryConfig : ScriptableObject
{
    [SerializeField] private string _alphabet;
    [SerializeField] private TextAsset _dictionaryFile;

    public string Alphabet => _alphabet;
    public TextAsset DictionaryFile => _dictionaryFile;

    public void SetAlphabet(string alphabet)
        => _alphabet = alphabet;

    public void SetDictionaryFile(TextAsset file)
        => _dictionaryFile = file;
}
*/