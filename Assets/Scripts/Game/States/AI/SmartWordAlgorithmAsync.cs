using System;
using System.Collections.Generic;
using System.Linq;
using Core.Config;
using Core.Dictionary;
using Cysharp.Threading.Tasks;
using Game.Logic;
using UI.Components;

namespace Game.AI
{
    public sealed class SmartWordAlgorithmAsync : IAIAlgorithmAsync
    {
        private bool _timeExpired;

        public void TimeExpired() => _timeExpired = true;

        public async UniTask<AIWordResult> GetWordAsync(
            ComplexityAISettings settings,
            WordsFieldManager wordsFieldManager,
            DictionaryService dictionaryService)
        {
            _timeExpired = false;

            var items = wordsFieldManager.WordsFieldData.Items;
            int boardMaxLen = items.Count; // 25
            int minLen = 2;

            int lettersOnBoard = CountLettersOnBoard(items);
            int maxPossibleNow = Math.Min(boardMaxLen, lettersOnBoard + 1);

            IEnumerable<int> lengthsToTry;

            if (settings.СomplexityAiLevel == ComplexityAI.HARD)
            {
                // HARD: пытаемся от максимально возможного СЕЙЧАС (не от max в словаре)
                int start = Math.Max(minLen, maxPossibleNow);
                lengthsToTry = Enumerable.Range(minLen, start - minLen + 1).Reverse();
            }
            else
            {
                // EASY/NORMAL: строго <= WordLength
                int start = Math.Min(boardMaxLen, Math.Max(minLen, (int)settings.WordLength));
                lengthsToTry = Enumerable.Range(minLen, start - minLen + 1).Reverse();
            }

            var result = await WordSearchEngineAsync.FindWordAsync(
                fieldItems: items,
                wordsFieldManager: wordsFieldManager,
                getWordsOfLength: dictionaryService.GetWordsOfLength,
                lengthsToTryDesc: lengthsToTry,
                timeExpired: () => _timeExpired,
                wordAlreadyUsed: wordsFieldManager.WordExist,
                yieldEveryWordChecks: 250);

            if (!result.Success)
                return AIWordResult.Fail();

            // Применяем ход (мы всё ещё на main thread, потому что UniTask.Yield)
            items[result.InsertIndex].SetLetter(result.InsertChar.ToString());
            foreach (int idx in result.PathIndexes)
                items[idx].Highlight();

            return AIWordResult.Ok(result.Word);
        }

        private static int CountLettersOnBoard(IReadOnlyList<SelectableLetter> items)
        {
            int c = 0;
            for (int i = 0; i < items.Count; i++)
                if (!items[i].Empty())
                    c++;
            return c;
        }
    }
}
