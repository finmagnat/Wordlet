using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Core.Services
{
    public class LocalizationService : ILocalizationService
    {
        public async UniTask InitializeAsync()
        {
            await LocalizationSettings.InitializationOperation.Task;
            Debug.Log($"🌐 Localization initialized. Current language: {CurrentLocale.Identifier.Code}");
        }

        public Locale CurrentLocale => LocalizationSettings.SelectedLocale;

        public void SetLocale(string code)
        {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(code);
            if (locale != null)
                LocalizationSettings.SelectedLocale = locale;
            else
                Debug.LogWarning($"Locale not found: {code}");
        }

        public LocalizedString GetLocalizedString(string table, string key)
        {
            return new LocalizedString(table, key);
        }

        public async UniTask<string> GetLocalizedTextAsync(string table, string key)
        {
            var localizedString = new LocalizedString(table, key);
            return await localizedString.GetLocalizedStringAsync().Task;
        }
    }
}