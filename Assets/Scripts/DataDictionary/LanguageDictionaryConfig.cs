using UnityEngine;

namespace Core.DataDictionary
{
    [CreateAssetMenu(menuName = "Wordlet/Dictionary/Language Config", fileName = "LanguageDictionaryConfig")]
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
