using System;
using System.Collections.Generic;
using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.DataDictionary
{
    public class DictionaryService : IDictionaryService
    {
        private readonly AddressablesLoader _loader;

        private LanguageDictionaryConfig _config;
        private HashSet<string> _words;
        private Dictionary<int, List<string>> _wordsByLength;
        private string _alphabet;
        private bool _isLoaded;

        public string Alphabet => _alphabet; // Алфавит текущего словаря.
        public IReadOnlyCollection<string> Words => _words;

        public DictionaryService(AddressablesLoader loader)
        {
            _loader = loader;
        }

        /// <summary>
        /// Инициализация словаря для конкретного языка.
        /// </summary>
        public async UniTask InitializeAsync(LanguageDictionaryConfig config)
        {
            _config = config;
            _isLoaded = false;
            _words = new HashSet<string>();
            _wordsByLength = new Dictionary<int, List<string>>();

            // 1. Алфавит из SO
            _alphabet = _config.alphabet;

            // 2. Загружаем текстовый файл словаря по ключу Addressables
            if (string.IsNullOrWhiteSpace(_config.dictionaryAddressKey))
            {
                Debug.LogError("❌ Dictionary address key is empty in LanguageDictionaryConfig");
                return;
            }

            var textAsset = await _loader.LoadAssetAsync<TextAsset>(_config.dictionaryAddressKey);
            if (textAsset == null)
            {
                Debug.LogError($"❌ Failed to load dictionary TextAsset by key: {_config.dictionaryAddressKey}");
                return;
            }

            var lines = textAsset.text.Split('\n');
            foreach (var line in lines)
            {
                var w = line.Trim().ToUpperInvariant();
                if (!string.IsNullOrEmpty(w))
                    _words.Add(w);
            }

            BuildWordLengthIndex();
            _isLoaded = true;

            Debug.Log($"📘 Dictionary initialized. Lang: {_config.languageCode}, words: {_words.Count}");
        }

        /// <summary>
        /// Алфавит текущего словаря (как строка).
        /// </summary>
        public string GetAlphabet() => _alphabet ?? string.Empty;

        /// <summary>
        /// Индексируем слова по длине для быстрых выборок.
        /// </summary>
        private void BuildWordLengthIndex()
        {
            _wordsByLength.Clear();

            foreach (var word in _words)
            {
                int len = word.Length;
                if (!_wordsByLength.TryGetValue(len, out var list))
                {
                    list = new List<string>();
                    _wordsByLength[len] = list;
                }

                list.Add(word);
            }

            Debug.Log($"📊 Words indexed by length. Length groups: {_wordsByLength.Count}");
        }

        /// <summary>
        /// Вернуть случайное слово указанной длины.
        /// Если слов такой длины нет — возвращает null.
        /// </summary>
        public string GetRandomWord(uint wordLength)
        {
            if (!_isLoaded)
            {
                Debug.LogWarning("⚠ DictionaryService.GetRandomWord called before dictionary was loaded.");
                return null;
            }

            if (wordLength <= 0 || wordLength > int.MaxValue)
                return null;

            int len = (int)wordLength;

            if (!_wordsByLength.TryGetValue(len, out var list) || list == null || list.Count == 0)
                return null;

            int index = UnityEngine.Random.Range(0, list.Count);
            return list[index];
        }

        /// <summary>
        /// Получить все слова заданной длины (read-only).
        /// Если таких слов нет — возвращает пустой массив.
        /// </summary>
        public IReadOnlyList<string> GetWordsOfLength(int length)
        {
            if (!_isLoaded)
                return Array.Empty<string>();

            if (length <= 0)
                return Array.Empty<string>();

            if (_wordsByLength.TryGetValue(length, out var list) && list != null)
                return list;

            return Array.Empty<string>();
        }

        /// <summary>
        /// Проверка наличия слова в словаре.
        /// </summary>
        public bool Contains(string word)
        {
            if (!_isLoaded || string.IsNullOrWhiteSpace(word))
                return false;

            return _words.Contains(word.Trim().ToUpperInvariant());
        }
    }
}
