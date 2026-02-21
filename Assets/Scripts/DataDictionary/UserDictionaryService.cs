using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.DataDictionary
{
    public class UserDictionaryService
    {
        private readonly string _path;
        private HashSet<string> _userWords;

        public IReadOnlyCollection<string> Words => _userWords;

        public UserDictionaryService(string languageCode)
        {
            _path = Path.Combine(Application.persistentDataPath, $"user_dict_{languageCode}.json");
        }

        public async UniTask InitializeAsync()
        {
            if (!File.Exists(_path))
            {
                _userWords = new HashSet<string>();
                await SaveAsync();
                return;
            }

            var json = await File.ReadAllTextAsync(_path);
            var list = JsonUtility.FromJson<Wrapper>(json)?.words;
            _userWords = list != null 
                ? new HashSet<string>(list) 
                : new HashSet<string>();
        }

        public bool AddWord(string word)
        {
            var w = word.Trim().ToUpperInvariant();
            if (_userWords.Add(w))
            {
                SaveAsync().Forget();
                return true;
            }
            return false;
        }

        public bool Contains(string word)
        {
            return _userWords.Contains(word.ToUpperInvariant());
        }

        private async UniTask SaveAsync()
        {
            var json = JsonUtility.ToJson(new Wrapper { words = new List<string>(_userWords) });
            await File.WriteAllTextAsync(_path, json);
        }

        [System.Serializable]
        private class Wrapper { public List<string> words; }
    }
}