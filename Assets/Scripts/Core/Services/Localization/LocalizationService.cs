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

            Locale targetLocale = null;

            // 1️⃣ Проверяем ручной выбор игрока (если был)
            string saved = PlayerPrefs.GetString(PlayerPrefsKey.LocaleCurrent, string.Empty);

            if (!string.IsNullOrEmpty(saved))
            {
                targetLocale = LocalizationSettings.AvailableLocales.GetLocale(saved);
                if (targetLocale != null)
                {
                    Debug.Log($"🌐 Loaded player-selected locale: {targetLocale.Identifier.Code}");
                    ApplyLocale(targetLocale, save: false);   // Уже сохранён ранее
                    return;
                }
            }

            // 2️⃣ Unity Locale Selectors (System Locale Selector, CommandLine etc.)
            Locale selectorLocale = LocalizationSettings.SelectedLocale;
            if (selectorLocale != null)
            {
                Debug.Log($"🌐 Unity selector chose: {selectorLocale.Identifier.Code}");
                // ⛔ Не сохраняем — это автоопределение
                return;
            }

            // 3️⃣ Пытаемся подобрать язык системы вручную (если включён SystemLanguage)
            Locale systemLocale = FindLocaleBySystemLanguage();
            if (systemLocale != null)
            {
                Debug.Log($"🌐 Using system language: {systemLocale.Identifier.Code}");
                ApplyLocale(systemLocale, save: false); // Не сохраняем
                return;
            }

            // 4️⃣ Fallback — Project Locale Identifier
            Locale fallback = LocalizationSettings.ProjectLocale;
            if (fallback != null)
            {
                Debug.Log($"🌐 Using fallback locale: {fallback.Identifier.Code}");
                ApplyLocale(fallback, save: false); // Не сохраняем
                return;
            }

            Debug.LogWarning("❗ No locale found. Please check localization settings.");
        }

        public Locale CurrentLocale => LocalizationSettings.SelectedLocale;

        /// <summary>
        /// Метод вызывается только когда игрок вручную выбирает язык.
        /// </summary>
        public void SetLocale(LocaleIdentifier id)
        {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(id);

            if (locale != null)
            {
                Debug.Log($"🌐 Player selected locale: {locale.LocaleName} ({locale.Identifier.Code})");
                ApplyLocale(locale, save: true);  // ✔ сохраняем выбор игрока
            }
            else
            {
                Debug.LogWarning($"Locale not found: {id}");
            }
        }

        private void ApplyLocale(Locale locale, bool save)
        {
            LocalizationSettings.SelectedLocale = locale;

            if (save)
            {
                PlayerPrefs.SetString(PlayerPrefsKey.LocaleCurrent, locale.Identifier.Code);
                PlayerPrefs.Save();
            }
        }

        private Locale FindLocaleBySystemLanguage()
        {
            string sysName = Application.systemLanguage.ToString();

            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                // Сравнение по названию
                if (locale.LocaleName.Equals(sysName, System.StringComparison.OrdinalIgnoreCase))
                    return locale;

                // Сравнение по ISO-коду
                if (locale.Identifier.Code.Equals(sysName, System.StringComparison.OrdinalIgnoreCase))
                    return locale;
            }
            return null;
        }

        public List<Locale> GetAvailableLocales() =>
            LocalizationSettings.AvailableLocales.Locales;

        public List<string> GetAvailableLocaleCodes()
        {
            var result = new List<string>();
            foreach (var l in LocalizationSettings.AvailableLocales.Locales)
                result.Add(l.Identifier.Code);
            return result;
        }

        public List<string> GetAvailableLocaleNames()
        {
            var result = new List<string>();
            foreach (var l in LocalizationSettings.AvailableLocales.Locales)
                result.Add(l.LocaleName);
            return result;
        }

        public LocalizedString GetLocalizedString(string table, string key) =>
            new LocalizedString(table, key);

        public async UniTask<string> GetLocalizedTextAsync(string table, string key)
        {
            var str = new LocalizedString(table, key);
            return await str.GetLocalizedStringAsync().Task;
        }
    }
}
