using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Logic;
using UI.Components;

namespace Game.AI
{
    public static class WordSearchEngineAsync
    {
        public readonly struct SearchResult
        {
            public readonly bool Success;
            public readonly string Word;
            public readonly int InsertIndex;
            public readonly char InsertChar;
            public readonly List<int> PathIndexes;

            public SearchResult(bool success, string word, int insertIndex, char insertChar, List<int> path)
            {
                Success = success;
                Word = word;
                InsertIndex = insertIndex;
                InsertChar = insertChar;
                PathIndexes = path;
            }

            public static SearchResult Fail() => new(false, string.Empty, -1, default, new List<int>());
        }

        public static async UniTask<SearchResult> FindWordAsync(
            List<SelectableLetter> fieldItems,
            WordsFieldManager wordsFieldManager,
            Func<int, IReadOnlyList<string>> getWordsOfLength,
            IEnumerable<int> lengthsToTryDesc,
            Func<bool> bCancelFunc,
            Func<string, bool> wordAlreadyUsed,
            int yieldEveryWordChecks = 250)
        {
            var insertCells = new List<int>(fieldItems.Count);
            for (int i = 0; i < fieldItems.Count; i++)
                if (wordsFieldManager.TrySetLetter(i))
                    insertCells.Add(i);

            if (insertCells.Count == 0)
                return SearchResult.Fail();

            int checkedWords = 0;

            foreach (int len in lengthsToTryDesc)
            {
                var words = getWordsOfLength(len);
                if (words == null || words.Count == 0)
                    continue;

                foreach (var raw in words)
                {
                    if (bCancelFunc())
                        return SearchResult.Fail();

                    // ✅ периодически отдаём кадр, чтобы таймер не фризил
                    if (++checkedWords % yieldEveryWordChecks == 0)
                        await UniTask.Yield();

                    var word = raw.Trim().ToUpperInvariant();
                    if (word.Length != len) continue;
                    if (wordAlreadyUsed(word)) continue;

                    for (int mid = 0; mid < word.Length; mid++)
                    {
                        char insertChar = word[mid];

                        foreach (int insertIndex in insertCells)
                        {
                            var used = new List<int>(word.Length) { insertIndex };

                            if (!BuildPart(fieldItems, used, insertIndex, word, mid - 1, -1))
                                continue;

                            if (!BuildPart(fieldItems, used, insertIndex, word, mid + 1, +1))
                                continue;

                            if (used.Count == word.Length)
                                return new SearchResult(true, word, insertIndex, insertChar, used);
                        }
                    }
                }
            }

            return SearchResult.Fail();
        }

        private static bool BuildPart(
            IReadOnlyList<SelectableLetter> fieldItems,
            List<int> used,
            int startIndex,
            string word,
            int nextIndex,
            int direction)
        {
            if (nextIndex < 0 || nextIndex >= word.Length)
                return true;

            return Dfs(fieldItems, used, startIndex, word, nextIndex, direction);
        }

        private static bool Dfs(
            IReadOnlyList<SelectableLetter> fieldItems,
            List<int> used,
            int currentIndex,
            string word,
            int nextWordIndex,
            int direction)
        {
            if (nextWordIndex < 0 || nextWordIndex >= word.Length)
                return true;

            char target = word[nextWordIndex];

            foreach (int n in GetNeighbors(currentIndex, fieldItems.Count))
            {
                if (!IsSameRowIfHorizontal(currentIndex, n))
                    continue;

                if (used.Contains(n))
                    continue;

                if (fieldItems[n].GetChar() != target)
                    continue;

                used.Add(n);
                if (Dfs(fieldItems, used, n, word, nextWordIndex + direction, direction))
                    return true;

                used.RemoveAt(used.Count - 1);
            }

            return false;
        }

        private static IEnumerable<int> GetNeighbors(int index, int count)
        {
            int up = index - 5;
            int down = index + 5;
            int left = index - 1;
            int right = index + 1;

            if (up >= 0) yield return up;
            if (down < count) yield return down;
            if (left >= 0) yield return left;
            if (right < count) yield return right;
        }

        private static bool IsSameRowIfHorizontal(int a, int b)
        {
            int diff = a - b;
            if (diff != 1 && diff != -1)
                return true;

            return (a / 5) == (b / 5);
        }
    }
}
