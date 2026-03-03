using System.Collections.Generic;
using Core.Config;
using UI.Components;
using UnityEngine;

namespace Core.Data
{
    public class WordsFieldData
    {
        public const int AMOUNT_LETTERS = 25;

        public List<SelectableLetter> Items => _items;

        private List<SelectableLetter> _items; // Список всех элементов на поле.
        private List<int> _selectedIndex = new(); // Индексы выбранных букв составляющих слово.
        private SelectableLetter _selectedItem; // Выделенная пустая ячейка (для будущей установки в нее буквы).
        private SelectableLetter _setItem; // Установленная буква.
        private List<string> _words = new(); // Список слов собранных в текущей игровой сессии.

        internal void SetItems (List<SelectableLetter> items) {
            _items = items;
        }
        
        /// <summary>
        /// Установить первое слово в среднюю строку поля.
        /// </summary>
        /// <param name="word">Слово</param>
        internal void SetFirstWord(string word)
        {
            _words.Add(word);
            for (int i = 10, n = 0; i < 15; ++i, ++n)
            {
                _items[i].SetLetter(word[n].ToString());
                _items[i].UnHighlight();
            }
        }
        
        /// <summary>
        /// Проверить слово на соответствие правилам игры
        /// </summary>
        /// <param name="word">Слово</param>
        /// <returns>Успех</returns>
        internal GameError CheckWord(string word)
        {   
            if (_selectedIndex.Count == 0) return GameError.WORD_NO_SELECTED;
            if (!_selectedIndex.Exists(x => x == _setItem.Index)) return GameError.SET_LETTER_NO_SELECTED;            
            if (Exist(word)) return GameError.WORD_ALREADY_BEEN;            
            return GameError.NONE;
        }
        
        /// <summary>
        /// Добавить слово в список слов собранных в текущей игровой сессии.
        /// </summary>
        /// <param name="word">Слово</param>
        internal void SaveWord(string word)
        {
            _words.Add(word);
        }
        
        /// <summary>
        /// Проверить, можно ли установить букву в текущую позицию.
        /// </summary>
        /// <param name="position">Позиция центра элемента с буквой "брошенного" игроком над полем</param>
        /// <param name="letter">Устанавливаемая буква</param>
        /// <returns>Успех</returns>
        internal bool CheckSetLetter(Vector3 position, string letter)
        {
            int index = 0;
            foreach (var item in _items)
            {
                if (index == 5)
                {
                    int a = 0;
                }
                if (item.HitTest(position) && TrySetLetter(index))
                {
                    item.SetLetter(letter);
                    _setItem = item;
                    return true;
                }
                ++index;
            }
            return false;
        }
               
        internal void BlinkNoSelectedLetter()
        {
            if (_setItem)
                _setItem.SetModeBlink();
        }

        internal void ShowLetters(bool value)
        {
            _items.ForEach(item => item.ShowLetter(value));
        }

        internal bool Exist(string word)
        {
            return _words.Exists(x => x == word);
        }

        /// <summary>
        /// Проверить, можно ли игроку выбрать данную букву на поле
        /// </summary>
        /// <param name="item">Выбранный игроком элемент поля</param>
        /// <returns>Успех</returns>
        internal bool CheckSelectLetter(SelectableLetter item)
        {
            int index = item.Index;            
            // Повторно нажал одну и ту же кнопку - не брать данные с кнопки.
            if(_selectedIndex.Exists(x => x == index))
                return false;

            // На кнопке нет буквы
            if (_items[index].Empty())
                return false;

            // Это первая выбранная буква
            if (_selectedIndex.Count == 0)
            {
                _selectedIndex.Add(index);
                return true; // Дальше сверяться не с чем
            }

            int indexLast = _selectedIndex[_selectedIndex.Count - 1];

            // Если предыдущая кнопка по отношению к текущей ...
            if (index - 5 >= 0) // сверху
                if (indexLast == index - 5)
                {
                    _selectedIndex.Add(index);
                    return true;
                }
            if (index - 1 >= 0) // слева
                if (indexLast == index - 1)
                {
                    _selectedIndex.Add(index);
                    return true;
                }
            if (index + 1 < AMOUNT_LETTERS) // справа
                if (indexLast == index + 1)
                {
                    _selectedIndex.Add(index);
                    return true;
                }
            if (index + 5 < AMOUNT_LETTERS) // снизу
                if (indexLast == index + 5)
                {
                    _selectedIndex.Add(index);
                    return true;
                }

            return false; // ...нажата по диагонали или дальше чем на одну позицию, не брать данные с кнопки.
        }
        
        /// <summary>
        /// Полностью ли заполнено поле.
        /// </summary>
        /// <returns>Успех</returns>
        internal bool Filled()
        {
            foreach (var item in _items)
            {
                if (item.Empty()) {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Проверка попытки игрока установить новую букву в желаемую позицию.
        /// </summary>
        /// <param name="index">Позиция элемента на поле</param>
        /// <returns>Успех</returns>
        internal bool TrySetLetter(int index)
        {
            if ((index < 0 || index >= _items.Count) || !_items[index].Empty()) // Целевая позиция должна быть свободна
                return false;

            // Рядом есть другая буква
            if (index - 5 >= 0)
                if (!_items[index - 5].Empty())
                    return true; // сверху

            if (index - 1 >= 0)
                if (!_items[index - 1].Empty())
                    return true; // слева

            if (index + 1 < AMOUNT_LETTERS)
                if (!_items[index + 1].Empty())
                    return true; // справа

            if (index + 5 < AMOUNT_LETTERS)
                if (!_items[index + 5].Empty())
                    return true; // снизу

            return false;
        }

        internal void SetSelectedIndexes(List<int> selectedIndex)
        {
            _selectedIndex = selectedIndex;
        }
        
        internal void SetLetterItem(SelectableLetter item)
        {
            _setItem = item;
        }
        
        internal void SetSelectedCell(SelectableLetter item)
        {
            _selectedItem = item;
        }
        
        internal bool SetLetterToSelectedCell(string letter)
        {
            if (_selectedItem != null)
            {
                _selectedItem.SetLetter(letter);
                _selectedItem.HighlightCell();
                _setItem = _selectedItem;
                _selectedItem = null;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Смыть выделение.
        /// </summary>
        internal void Clear()
        {
            _items?.ForEach(item => item.UnHighlight());
            _selectedIndex.Clear();
        }
        
        /// <summary>
        /// Отменить.
        /// Убирается выделение и установленая буква.
        /// </summary>
        internal void Cancel()
        {            
            _setItem.SetLetter("");
            Clear();
        }

        internal void Reset()
        {
            _items?.ForEach(item => item.SetLetter(""));
            Clear();
            _words.Clear();
            _setItem = null;
        }
        
        internal bool BacktrackOneStep(out SelectableLetter removedItem)
        {
            removedItem = null;

            if (_selectedIndex == null || _selectedIndex.Count == 0)
                return false;

            int lastIndex = _selectedIndex[_selectedIndex.Count - 1];
            _selectedIndex.RemoveAt(_selectedIndex.Count - 1);

            removedItem = _items[lastIndex];
            return true;
        }

        public string[] GetBoardData()
        {
            string[] list = new string[AMOUNT_LETTERS];

            for (int i = 0; i < AMOUNT_LETTERS; ++i)
                list[i] = _items[i].GetLetter();
            
            return list;
        }

        public void SetSaveGameData(SaveGameData saveGameData)
        {
            _words.Add(saveGameData.firstWord);
            for (int i = 0; i < saveGameData.boardRows.Length; ++i)
            {
                _items[i].SetLetter(saveGameData.boardRows[i]);
                _items[i].UnHighlight();
            }

            _words.AddRange(saveGameData.playerWords);
            _words.AddRange(saveGameData.opponentWords);
        }
    }
}