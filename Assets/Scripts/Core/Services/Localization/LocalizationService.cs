using System.Collections.Generic;
using Core.Config;
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

        public void SetLocale(LocaleIdentifier id)
        {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(id);
            if (locale != null)
            {
                LocalizationSettings.SelectedLocale = locale;
                
                PlayerPrefs.SetString(PlayerPrefsKey.LocaleCurrent, locale.Identifier.Code);
                PlayerPrefs.Save();
                
                Debug.Log($"Selected Locale: {id}");
            }
            else
                Debug.LogWarning($"Locale not found: {id}");
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
        
        public List<Locale> GetAvailableLocales()
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            return locales;
        }

        public List<string> GetAvailableLocaleCodes()
        {
            var codes = new List<string>();
            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                codes.Add(locale.Identifier.Code); // например "en", "ru", "fr"
            }
            return codes;
        }

        public List<string> GetAvailableLocaleNames()
        {
            var names = new List<string>();
            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                names.Add(locale.LocaleName); // например "English", "Русский"
            }
            return names;
        }
    }
}