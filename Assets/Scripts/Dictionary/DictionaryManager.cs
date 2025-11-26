using System.Collections.Generic;
using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using UnityEngine.Localization;

namespace Core.Dictionary
{
    public class DictionaryManager
    {
        private readonly DictionaryService _dictionaryService;
        private readonly LocalizationService _localization;
        private readonly Dictionary<string, LanguageDictionaryConfig> _configs;

        public DictionaryService Service => _dictionaryService;

        [Inject]
        public DictionaryManager(
            DictionaryService dictionaryService,
            LocalizationService localization,
            List<LanguageDictionaryConfig> configs)
        {
            _dictionaryService = dictionaryService;
            _localization = localization;

            _configs = new Dictionary<string, LanguageDictionaryConfig>();
            foreach (var cfg in configs)
                _configs[cfg.languageCode] = cfg;

            // подписываемся на смену языка
            _localization.OnLocaleChanged += OnLocaleChanged;
        }

        public async UniTask InitializeAsync()
        {
            await LoadDictionaryForCurrentLocale();
        }

        private async void OnLocaleChanged(Locale _)
        {
            await LoadDictionaryForCurrentLocale();
        }

        private async UniTask LoadDictionaryForCurrentLocale()
        {
            string code = _localization.CurrentLocale.Identifier.Code;

            if (!_configs.TryGetValue(code, out var cfg))
            {
                Debug.LogError($"❌ No dictionary config for locale '{code}'");
                return;
            }

            await _dictionaryService.InitializeAsync(cfg);
            Debug.Log($"📚 Dictionary switched → {code}");
        }
    }
}