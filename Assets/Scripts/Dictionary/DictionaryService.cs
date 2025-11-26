using System.Collections.Generic;
using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Dictionary
{
    public class DictionaryService
    {
        private readonly AddressablesLoader _loader;
        
        private LanguageDictionaryConfig _config;
        private HashSet<string> _words;
        private string _alphabet;

        public DictionaryService(AddressablesLoader loader)
        {
            _loader = loader;
        }

        public async UniTask InitializeAsync(LanguageDictionaryConfig config)
        {
            _config = config;
            
            // 1. Алфавит просто берём из конфига
            _alphabet = _config.alphabet;

            // 2. Загружаем текстовый словарь по Addressables-ключу
            if (string.IsNullOrWhiteSpace(_config.dictionaryAddressKey))
            {
                Debug.LogError("❌ Dictionary address key is empty in LanguageDictionaryConfig");
                _words = new HashSet<string>();
                return;
            }

            var textAsset = await _loader.LoadAssetAsync<TextAsset>(_config.dictionaryAddressKey);
            if (textAsset == null)
            {
                Debug.LogError($"❌ Failed to load dictionary TextAsset by key: {_config.dictionaryAddressKey}");
                _words = new HashSet<string>();
                return;
            }

            var lines = textAsset.text.Split('\n');
            _words = new HashSet<string>();

            foreach (var line in lines)
            {
                var w = line.Trim().ToUpperInvariant();
                if (!string.IsNullOrEmpty(w))
                    _words.Add(w);
            }

            Debug.Log($"📘 Dictionary initialized. Lang: {_config.languageCode}, words: {_words.Count}");
        }

        /// <summary>
        /// Алфавит текущего словаря (как строка).
        /// </summary>
        public string GetAlphabet()
        {
            return _alphabet ?? string.Empty;
        }

        /// <summary>
        /// Быстрая проверка, есть ли слово в словаре.
        /// </summary>
        public bool Contains(string word)
        {
            if (string.IsNullOrWhiteSpace(word) || _words == null)
                return false;

            return _words.Contains(word.Trim().ToUpperInvariant());
        }
    }
}
