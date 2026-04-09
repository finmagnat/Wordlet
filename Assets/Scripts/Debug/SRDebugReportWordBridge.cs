using System;
using System.Linq;
using Core.Services.NewWords;
using Core.Services.ReportWord;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.DebugTools
{
    public sealed class SRDebugReportWordBridge : MonoBehaviour
    {
        [Inject] private IReportWordService _reportWordService;

        public static SRDebugReportWordBridge Instance { get; private set; }

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
                var language = DebugLanguageCode.Get();
                var words = await _reportWordService.GetPendingWordsAsync(language);

                if (words == null || words.Count == 0)
                {
                    Debug.Log($"[ReportWord] pending_words_{language}: EMPTY");
                    return;
                }

                var joined = string.Join(", ", words.Select(x => x.word));
                Debug.Log($"[ReportWord] pending_words_{language} ({words.Count}): {joined}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ReportWord] DumpPendingWordsAsync failed: {e}");
            }
        }

        private async UniTaskVoid DumpPendingWordsToClipboardAsync()
        {
            try
            {
                var language = DebugLanguageCode.Get();
                var words = await _reportWordService.GetPendingWordsAsync(language);

                if (words == null || words.Count == 0)
                {
                    GUIUtility.systemCopyBuffer = string.Empty;
                    Debug.Log($"[ReportWord] Clipboard cleared. pending_words_{language} is EMPTY.");
                    return;
                }

                var text = string.Join("\n", words
                    .Where(x => x != null && !string.IsNullOrWhiteSpace(x.word))
                    .Select(x => x.word)
                    .OrderBy(x => x));

                GUIUtility.systemCopyBuffer = text;

                Debug.Log($"[ReportWord] Copied {words.Count} words to clipboard for '{language}'.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ReportWord] DumpPendingWordsToClipboardAsync failed: {e}");
            }
        }

        private async UniTaskVoid AddPendingWordAsync()
        {
            try
            {
                var language = DebugLanguageCode.Get();
                var rawWord = WordToAdd;

                if (string.IsNullOrWhiteSpace(rawWord))
                {
                    Debug.LogWarning("[ReportWord] WordToAdd is empty.");
                    return;
                }

                var result = await _reportWordService.SubmitWordAsync(rawWord, language);

                Debug.Log($"[ReportWord] Add result: status={result.status}, word={result.normalizedWord}, language={language}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ReportWord] AddPendingWordAsync failed: {e}");
            }
        }

        private async UniTaskVoid ClearAllPendingWordsAsync()
        {
            try
            {
                var language = DebugLanguageCode.Get();
                var result = await _reportWordService.ClearPendingWordsAsync(language);

                Debug.Log($"[ReportWord] Clear all result: status={result.status}, language={result.language}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ReportWord] ClearAllPendingWordsAsync failed: {e}");
            }
        }

    }
}