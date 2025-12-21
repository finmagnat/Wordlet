using System;
using System.Collections.Generic;
using Core.Config;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Core.Services
{
    /// <summary>
    /// Получить локализированный текст синхронно:
    /// string txt = _loc.Get("Dynamic_Texts", "PASSES", 3, 5);
    /// 
    /// Асинхронно:
    /// string txt = await _loc.GetAsync("Dynamic_Texts", "PASSES", 3, 5);
    /// 
    /// Изменить язык:
    /// _loc.SetLocale("en");
    /// 
    /// Реагировать на смену языка:
    /// _loc.OnLocaleChanged += locale =>
    /// {
    ///     Debug.Log("Language switched: " + locale.Identifier.Code);
    ///     тут можно обновить UI
    /// };
    /// 
    /// Получить список поддерживаемых языков:
    /// var codes = _loc.GetAvailableLocaleCodes();
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        private readonly Dictionary<string, string> _cache = new();

        public Locale CurrentLocale => LocalizationSettings.SelectedLocale;

        public event Action<Locale> OnLocaleChanged;

        // ---------------------------------------------------------
        // INITIALIZATION
        // ---------------------------------------------------------

        public async UniTask InitializeAsync()
        {
            await LocalizationSettings.InitializationOperation.Task;

            Debug.Log($"🌐 Localization initialized. Available: {LocalizationSettings.AvailableLocales.Locales.Count}");

            string savedCode = PlayerPrefs.GetString(PlayerPrefsKey.LocaleCurrent, string.Empty);

            if (!string.IsNullOrEmpty(savedCode))
            {
                SetLocale(savedCode);
                return;
            }

            // system language
            Locale system = LocalizationSettings.AvailableLocales.GetLocale(Application.systemLanguage);
            if (system != null)
            {
                SetLocale(system.Identifier.Code);
                return;
            }

            // fallback (Project Locale Identifier)
            SetLocale(LocalizationSettings.ProjectLocale.Identifier.Code);
        }

        // ---------------------------------------------------------
        // LOCALE SET
        // ---------------------------------------------------------

        public void SetLocale(string code)
        {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(code);
            if (locale == null)
            {
                Debug.LogWarning($"⚠ Locale not found: {code}");
                return;
            }

            LocalizationSettings.SelectedLocale = locale;

            PlayerPrefs.SetString(PlayerPrefsKey.LocaleCurrent, code);
            PlayerPrefs.Save();

            _cache.Clear();
            OnLocaleChanged?.Invoke(locale);

            Debug.Log($"🌐 Locale changed to: {locale.Identifier.Code}");
        }

        // ---------------------------------------------------------
        // ASYNCHRONOUS LOCALIZED STRING
        // ---------------------------------------------------------

        public async UniTask<string> GetAsync(string table, string key, params object[] args)
        {
            string cacheKey = $"{table}/{key}/{string.Join(",", args)}";

            if (_cache.TryGetValue(cacheKey, out var cached))
                return cached;

            var loc = new LocalizedString(table, key);
            if (args != null && args.Length > 0)
                loc.Arguments = args;

            string result = await loc.GetLocalizedStringAsync().Task;

            _cache[cacheKey] = result;
            return result;
        }

        // ---------------------------------------------------------
        // SYNCHRONOUS LOCALIZED STRING
        // ---------------------------------------------------------

        public string Get(string table, string key, params object[] args)
        {
            string cacheKey = $"{table}/{key}/{string.Join(",", args)}";

            if (_cache.TryGetValue(cacheKey, out var cached))
                return cached;

            var loc = new LocalizedString(table, key);
            if (args != null && args.Length > 0)
                loc.Arguments = args;

            string result = loc.GetLocalizedString(); // sync

            _cache[cacheKey] = result;
            return result;
        }

        // ---------------------------------------------------------
        // AVAILABLE LOCALES
        // ---------------------------------------------------------

        public List<Locale> GetAvailableLocales() =>
            LocalizationSettings.AvailableLocales.Locales;

        public List<string> GetAvailableLocaleCodes()
        {
            var list = new List<string>();
            foreach (var loc in LocalizationSettings.AvailableLocales.Locales)
                list.Add(loc.Identifier.Code);
            return list;
        }

        public List<string> GetAvailableLocaleNames()
        {
            var list = new List<string>();
            foreach (var loc in LocalizationSettings.AvailableLocales.Locales)
                list.Add(loc.LocaleName);
            return list;
        }
    }
}
