using Cysharp.Threading.Tasks;
using Zenject;

namespace Core.Dictionary
{
    public class DictionaryManager
    {
        private readonly DictionaryService _mainDict;
        private UserDictionaryService _userDict;

        public DictionaryManager(DictionaryService mainDict)
        {
            _mainDict = mainDict;
        }

        public async UniTask InitializeAsync(LanguageDictionaryConfig config)
        {
            await _mainDict.InitializeAsync(config);

            _userDict = new UserDictionaryService(config.languageCode);
            await _userDict.InitializeAsync();
        }

        public bool Contains(string word)
        {
            return _mainDict.Contains(word) || _userDict.Contains(word);
        }

        public bool AddCustomWord(string word)
        {
            return _userDict.AddWord(word);
        }
    }
}