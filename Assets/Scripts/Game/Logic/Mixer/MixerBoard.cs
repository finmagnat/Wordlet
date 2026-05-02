using System.Collections.Generic;
using Core.Data;

namespace Game.Logic.Mixer
{
    public sealed class MixerBoard
    {
        public const int Width = 5;
        public const int Height = 5;
        public const int CellCount = WordsFieldData.AMOUNT_LETTERS;

        private readonly string[] _boardData = new string[CellCount];
        private readonly List<MixerLetter> _letters = new();

        public IReadOnlyList<MixerLetter> Letters => _letters;
        public int FilledCount => _letters.Count;

        public MixerBoard(IReadOnlyList<string> boardData)
        {
            for (int i = 0; i < CellCount; ++i)
            {
                string letter = boardData != null && i < boardData.Count ? boardData[i] : string.Empty;
                _boardData[i] = letter ?? string.Empty;

                if (!string.IsNullOrEmpty(_boardData[i]))
                    _letters.Add(new MixerLetter(i, _boardData[i]));
            }
        }

        public string GetLetter(int index)
        {
            if (!IsValidIndex(index))
                return string.Empty;

            return _boardData[index];
        }

        public bool HasLetterAt(int index)
        {
            return !string.IsNullOrEmpty(GetLetter(index));
        }

        public bool IsSameBoard(IReadOnlyList<string> boardData)
        {
            if (boardData == null || boardData.Count != CellCount)
                return false;

            for (int i = 0; i < CellCount; ++i)
            {
                if (_boardData[i] != (boardData[i] ?? string.Empty))
                    return false;
            }

            return true;
        }

        public static bool IsValidIndex(int index)
        {
            return index >= 0 && index < CellCount;
        }

        public static int ToIndex(int row, int column)
        {
            return row * Width + column;
        }

        public static int GetRow(int index)
        {
            return index / Width;
        }

        public static int GetColumn(int index)
        {
            return index % Width;
        }

        public static IEnumerable<int> GetOrthogonalNeighbors(int index)
        {
            int row = GetRow(index);
            int column = GetColumn(index);

            if (row > 0)
                yield return ToIndex(row - 1, column);

            if (row < Height - 1)
                yield return ToIndex(row + 1, column);

            if (column > 0)
                yield return ToIndex(row, column - 1);

            if (column < Width - 1)
                yield return ToIndex(row, column + 1);
        }
    }
}
