using System;
using System.Linq;
using Core.Services.NewWords;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.DebugTools
{
    public enum DebugNewWordsLanguage
    {
        Ru,
        En,
        Uk
    }

    public sealed class SRDebugNewWordsBridge : MonoBehaviour
    {
        [Inject] private INewWordsService _newWordsService;

        public static SRDebugNewWordsBridge Instance { get; private set; }

        public DebugNewWordsLanguage SelectedLanguage { get; set; } = DebugNewWordsLanguage.Ru;
        public string WordToAdd { get; set; } = string.Empty;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        [ContextMenu("Dump Pending Words")]
        public void DumpPendingWords()
        {
            DumpPendingWordsAsync().Forget();
        }

        [ContextMenu("Dump Pending Words To Clipboard")]
        public void DumpPendingWordsToClipboard()
        {
            DumpPendingWordsToClipboardAsync().Forget();
        }

        [ContextMenu("Add Pending Word")]
        public void AddPendingWord()
        {
            AddPendingWordAsync().Forget();
        }

        [ContextMenu("Clear All Pending Words")]
        public void ClearAllPendingWords()
        {
            ClearAllPendingWordsAsync().Forget();
        }

        private async UniTaskVoid DumpPendingWordsAsync()
        {
            try
            {
                var language = GetLanguageCode();
                var words = await _newWordsService.GetPendingWordsAsync(language);

                if (words == null || words.Count == 0)
                {
                    Debug.Log($"[NewWords] pending_words_{language}: EMPTY");
                    return;
                }

                var joined = string.Join(", ", words.Select(x => x.word));
                Debug.Log($"[NewWords] pending_words_{language} ({words.Count}): {joined}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[NewWords] DumpPendingWordsAsync failed: {e}");
            }
        }

        private async UniTaskVoid DumpPendingWordsToClipboardAsync()
        {
            try
            {
                var language = GetLanguageCode();
                var words = await _newWordsService.GetPendingWordsAsync(language);

                if (words == null || words.Count == 0)
                {
                    GUIUtility.systemCopyBuffer = string.Empty;
                    Debug.Log($"[NewWords] Clipboard cleared. pending_words_{language} is EMPTY.");
                    return;
                }

                var text = string.Join("\n", words
                    .Where(x => x != null && !string.IsNullOrWhiteSpace(x.word))
                    .Select(x => x.word)
                    .OrderBy(x => x));

                GUIUtility.systemCopyBuffer = text;

                Debug.Log($"[NewWords] Copied {words.Count} words to clipboard for '{language}'.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[NewWords] DumpPendingWordsToClipboardAsync failed: {e}");
            }
        }

        private async UniTaskVoid AddPendingWordAsync()
        {
            try
            {
                var language = GetLanguageCode();
                var rawWord = WordToAdd;

                if (string.IsNullOrWhiteSpace(rawWord))
                {
                    Debug.LogWarning("[NewWords] WordToAdd is empty.");
                    return;
                }

                var result = await _newWordsService.SubmitWordAsync(rawWord, language);

                Debug.Log($"[NewWords] Add result: status={result.status}, word={result.normalizedWord}, language={language}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[NewWords] AddPendingWordAsync failed: {e}");
            }
        }

        private async UniTaskVoid ClearAllPendingWordsAsync()
        {
            try
            {
                var language = GetLanguageCode();
                var result = await _newWordsService.ClearPendingWordsAsync(language);

                Debug.Log($"[NewWords] Clear all result: status={result.status}, language={result.language}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[NewWords] ClearAllPendingWordsAsync failed: {e}");
            }
        }

        public string GetLanguageCode()
        {
            return SelectedLanguage switch
            {
                DebugNewWordsLanguage.Ru => "ru",
                DebugNewWordsLanguage.En => "en",
                DebugNewWordsLanguage.Uk => "uk",
                _ => "ru"
            };
        }
    }
}