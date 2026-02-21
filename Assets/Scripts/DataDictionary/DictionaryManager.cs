using System.Collections.Generic;
using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using UnityEngine.Localization;

namespace Core.DataDictionary
{
    public class DictionaryManager
    {
        private readonly DictionaryService _dictionaryService;
        private readonly LocalizationService _localization;
        private readonly Dictionary<string, LanguageDictionaryConfig> _configs = new();

        public DictionaryService Service => _dictionaryService;

        [Inject]
        public DictionaryManager(
            DictionaryService dictionaryService,
            LocalizationService localization,
            List<LanguageDictionaryConfig> configs)
        {
            _dictionaryService = dictionaryService;
            _localization = localization;
            
            foreach (var cfg in configs)
                _configs[cfg.languageCode] = cfg;
        }

        public async UniTask InitializeAsync()
        {
            // подписываемся на смену языка
            _localization.OnLocaleChanged += OnLocaleChanged;
            
            await LoadDictionaryForCurrentLocale();
        }

        public void Destroy()
        {
            _localization.OnLocaleChanged -= OnLocaleChanged;
        }

        private async void OnLocaleChanged(Locale _)
        {
            await LoadDictionaryForCurrentLocale();
        }

        private async UniTask LoadDictionaryForCurrentLocale()
        {
            string code = _localization.CurrentLocale.Identifier.Code;

            if (!TryGetConfigForLocale(code, out var cfg))
            {
                Debug.LogError($"❌ No dictionary config for locale '{code}' (also tried language fallback).");
                return;
            }

            await _dictionaryService.InitializeAsync(cfg);
            Debug.Log($"📚 Dictionary switched → {cfg.languageCode} (locale was {code})");
        }
        
        private bool TryGetConfigForLocale(string localeCode, out LanguageDictionaryConfig cfg)
        {
            // 1) точное совпадение: "en-US"
            if (_configs.TryGetValue(localeCode, out cfg))
                return true;

            // 2) fallback по языку: "en-US" -> "en"
            var dash = localeCode.IndexOf('-');
            if (dash > 0)
            {
                var langOnly = localeCode.Substring(0, dash);
                if (_configs.TryGetValue(langOnly, out cfg))
                    return true;
            }

            cfg = null;
            return false;
        }

    }
}