using System;
using System.Collections.Generic;
using System.Linq;
using Core.Config;
using Core.DataDictionary;
using Cysharp.Threading.Tasks;
using Game.Logic;
using UI.Components;
using UnityEngine;
using Random = System.Random;

namespace Game.AI
{
    public sealed class SmartWordAlgorithmAsync : IAIAlgorithmAsync
    {
        private static readonly Random _rng = new();
        
        private bool _bCancel;

        public void Cancel() => _bCancel = true;

        public async UniTask<AIWordResult> GetWordAsync(
            ComplexityAISettings settings,
            WordsFieldManager wordsFieldManager,
            DictionaryService dictionaryService)
        {
            _bCancel = false;

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
            else if (settings.СomplexityAiLevel == ComplexityAI.NORMAL)
            {
                // NORMAL: случайная длина между минимальной и максимально возможной
                int maxLen = Math.Max(minLen, maxPossibleNow);
                int randomStart = _rng.Next(minLen, maxLen + 1);

                // сначала пробуем randomStart, потом уменьшаем длину
                lengthsToTry = Enumerable.Range(minLen, randomStart - minLen + 1).Reverse();
            }
            else
            {
                // EASY строго <= WordLength
                int start = Math.Min(boardMaxLen, Math.Max(minLen, (int)settings.WordLength));
                lengthsToTry = Enumerable.Range(minLen, start - minLen + 1).Reverse();
            }
            
            //Debug.Log($"[SmartWordAlgorithmAsync][GetWordAsync] lengthsToTry[{lengthsToTry.Count()}] = {string.Join(", ", lengthsToTry.ToArray())}" );

            var result = await WordSearchEngineAsync.FindWordAsync(
                fieldItems: items,
                wordsFieldManager: wordsFieldManager,
                getWordsOfLength: dictionaryService.GetWordsOfLength,
                lengthsToTryDesc: lengthsToTry,
                bCancelFunc: () => _bCancel,
                wordAlreadyUsed: wordsFieldManager.WordExist,
                yieldEveryWordChecks: 250);

            if (!result.Success)
                return AIWordResult.Fail();

            // Применяем ход (мы всё ещё на main thread, потому что UniTask.Yield)
            items[result.InsertIndex].SetLetter(result.InsertChar.ToString());
            foreach (int idx in result.PathIndexes)
                items[idx].Highlight();
                
            wordsFieldManager.WordsFieldData.SetSelectedIndexes(result.PathIndexes);
            wordsFieldManager.WordsFieldData.SetLetterItem(items[result.InsertIndex]);

            Debug.Log($"[SmartWordAlgorithmAsync][GetWordAsync] Word [{result.Word.Count()}] = {result.Word}" );
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
