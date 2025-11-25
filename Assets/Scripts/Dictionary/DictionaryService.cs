using System.Collections.Generic;
using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Dictionary
{
    public class DictionaryService
    {
        private readonly AddressablesLoader _loader;

        private HashSet<string> _dictionary;
        private string _alphabet;

        public string Alphabet => _alphabet;
        public IReadOnlyCollection<string> Words => _dictionary;

        public DictionaryService(AddressablesLoader loader)
        {
            _loader = loader;
        }

        public async UniTask InitializeAsync(LanguageDictionaryConfig config)
        {
            _alphabet = config.alphabet;

            var file = await _loader.LoadAssetAsync<TextAsset>(config.dictionaryAddressKey);
            if (file == null)
            {
                Debug.LogError($"❌ Unable to load dictionary file: {config.dictionaryAddressKey}");
                _dictionary = new HashSet<string>();
                return;
            }

            var lines = file.text.Split('\n');
            _dictionary = new HashSet<string>();

            foreach (var l in lines)
            {
                var w = l.Trim().ToUpperInvariant();
                if (w.Length > 0)
                    _dictionary.Add(w);
            }

            Debug.Log($"📘 Loaded dictionary: {config.languageCode}, words: {_dictionary.Count}");
        }

        public bool Contains(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) 
                return false;
            return _dictionary.Contains(word.ToUpperInvariant());
        }
    }
}