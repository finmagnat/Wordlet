using System;
using System.Collections.Generic;
using System.Linq;
using Core.Config;
using Core.Dictionary;
using Game.Logic;
using UI.Components;

namespace Game.AI
{
    /// <summary>
    /// Алгоритм подбора слова для AI.
    /// 
    /// 1. Выбирается слово, начиная с количества рекомендованного настройками ComplexityAISettings для текущего уровня сложности
    /// вплоть до слов максимальной длины.
    /// 2. Первый символ слова поочередно ставится во все свободные позиции, если рядом есть другие символы.
    /// 3. Выполняется попытка вставки всего слова в соответствии с правилами игры.
    /// 4. Перебирается весь список слов установленной длины.
    /// 5. Если слово не удалось подобрать, этот же список проходится повторно, но для каждого слова выполняется реверс.
    /// 6. Последний символ слова поочередно ставится во все свободные/пригодные позиции. Выполняются шаги пунктов 3 и 4.
    /// 7. Если поиск не был успешным, с каждым разом уровень сложности понижается и выполняются шаги пунктов 2 по 6.
    /// 8. При полной неудаче подбора слова, алгоритм завершает работу возвращая false 
    /// (и передает возможность контроллеру высшего уровня применить другие алгоритмы).
    /// </summary>
    public class FirstLastCharacter : IAIAlgorithm
    {
        private ComplexityAISettings _settings; // Настройки текущего уровня сложности.
        private List<SelectableLetter> _fieldItems; // Список элементов поля, на которые выполняется установка букв/слов.
        private List<int> _wordIndexes = new (); // Индексы элементов, в которые "поставлены" буквы из word (по ним кнопки будут "выделены")
		private IReadOnlyCollection<string> _dictionary; // Словарь
		private string _word; // Найденное слово.
		private bool _bTimeExpired;
		private ComplexityAI _nLevel; // Уровень сложности для алгоритмов.
		private WordsFieldManager _wordsFieldManager;

		public void TimeExpired()
        {
			_bTimeExpired = true;
		}

		public bool GetWord(out string word, ComplexityAISettings settings, WordsFieldManager wordsFieldManager, DictionaryService dictionaryService)
        {
			_bTimeExpired = false;
			_settings = settings;
			_wordsFieldManager = wordsFieldManager;
            _dictionary = dictionaryService.Words;
            _fieldItems = _wordsFieldManager.WordsFieldData.Items;
			_nLevel = _settings.СomplexityAiLevel;

			bool bSuccess = false; // Cлово найдено.

			// Каскадный вызов алгоритмов.
			if (!Find())
			{
				if (!Find(true)) // Реверс слова.
				{	
					if (_nLevel == ComplexityAI.HARD)
					{
						_nLevel = ComplexityAI.NORMAL; // Снижение уровня сложности до NORMAL.
						if (!Find())
						{
							bSuccess = Find(true); // Реверс слова.		
						}
						else bSuccess = true;
					}

					if (_nLevel == ComplexityAI.NORMAL && !bSuccess)
					{
						_nLevel = ComplexityAI.EASY; // Снижение уровня сложности до EASY.
						if (!Find())
						{
							bSuccess = Find(true); // Реверс слова.
						}
						else bSuccess = true;
					}
				}
				else bSuccess = true;
			}
			else bSuccess = true;

			word = _word;

			if (bSuccess)
			{
				foreach (int index in _wordIndexes) // Выделение слова на поле.
				{
					_fieldItems[index].Highlight();
				}
				_wordIndexes.Clear();
			}
						
			return bSuccess; // Если слово не найдено, управление переходит к другому алгоритму либо засчитывается пропуск хода.
		}

		private bool Find(bool bReverse = false)
		{
			int wordLength = 2; // Минимальный размер слова.
			switch(_nLevel)
            {
				case ComplexityAI.EASY:
				case ComplexityAI.NORMAL:
				case ComplexityAI.HARD:
					//wordLength = CoreGameService.GameConfig.GetComplexityAIItem(_nLevel).WordLength;
					break;
			}

			for (int i = 0; i < _dictionary.Count; i++) // Поиск подходящего слова.
			{
				//_word = _dictionary[i];
				_word = _dictionary.ElementAt(i);
				// Подбирается слово по длине соответствующей уровню сложности или более.
				// И, которого еще не было в текущей игровой сессии.
				if (_word.Length < wordLength || _wordsFieldManager.WordExist(_word)) {
					continue; 
				}

				if (bReverse) {
					_word = ReverseString(_word);
				}

				int start = 5, end = 9; // 2-ой ряд
				for (int j = start; j <= end; j++) // Переход по элементам ряда.
				{
					if (_bTimeExpired) 
					{	
						return false;  // Кончилось время (пропуск хода).
					}

					if (_wordsFieldManager.TrySetLetter(j)) // Элемент пустой и рядом есть 1 или более символов.
					{							
						int pos = j, tempPos; // j менять нельзя
						_wordIndexes.Add(pos); // Условно первая буква сразу ставится на этот (базовый) пустой элемент.
														
						// Перебираем буквы слова и ищем им место - соседние элементы с такими же символами как в слове.
						for (int n = 1; n < _word.Length; n++) 
						{
							tempPos = SearchLetterAlongside(pos, _word[n]);
							if (tempPos != -1) // Соседний элемент с такой же буквой как в слове.
							{
								pos = tempPos; // Запоминаем его индекс (и переходим к следующей букве слова).
								_wordIndexes.Add(pos);
							}
							else // Нет рядом подходящей буквы (слово не установилось полностью на поле).
							{
								_wordIndexes.Clear(); // Очистка списка индексов.
								break; // Переходим к следующему базовому элементу.
							}
						}

						if (_wordIndexes.Count == _word.Length) // Всё слово установилось благополучно!
						{
							_fieldItems[j].SetLetter(new string(_word[0], 1)); // Первую букву ставим на базовый элемент.
																
							if (bReverse)
							{
								_word = ReverseString(_word); // Переворачиваем слово обратно.
							}
								
							return true; // Слово найдено (запрет на следующий алгоритм).
						}
					}
					if (j == 9) { j = 0; end = 4; }   // Переход на 1-ый ряд.
					if (j == 4) { j = 15; end = 24; }  // Переход на 4-ый и 5-ый ряд.
				}
			}
			return false; // Неудача (добро на следующий алгоритм).
		}
				
		private string ReverseString(string s)
		{			
			char[] array = s.ToCharArray();
			Array.Reverse(array);
			return new string(array);
		}


		/// <summary>
		/// Поиск в соседних элементах буквы слова.
		/// </summary>		
		/// <param name="index"></param>
		/// <param name="ch"></param>
		/// <returns>Позиция элемента с искомым символом</returns>
		int SearchLetterAlongside(int index, char ch) {
			int[] posSides = { index - 5/*сверху*/, index + 5/*снизу*/, index - 1/*слева*/, index + 1/*справа*/ };
			foreach (int neighborInd in posSides)
            {
				if (CheckLetterAlongside(index, neighborInd, ch))
				{
					return neighborInd;
				}
			}
			
			return -1; // Нет рядом такой буквы.
		}
		/// <summary>
		/// Проверка, находится ли в элементе поля указанном по индексу искомый символ.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="ch"></param>
		/// <returns>Успех</returns>
		bool CheckLetterAlongside(int baseIndex, int neighborInd, char ch)
		{
			if (neighborInd >= 0 && neighborInd < _fieldItems.Count && CheckIndexInRow(baseIndex, neighborInd))
			{
				if(!_wordIndexes.Contains(neighborInd)) // В эту позицию еще не "ставили" букву.
				{					
					if (_fieldItems[neighborInd].GetChar() == ch)
					{ 
						return true; // Элемент содержит искомый символ!
					}
				}
			}
			return false;
		}
		/// <summary>
		/// Определение, находится ли левый или правый элемент на том же ряду, что и базовый.
		/// </summary>
		/// <param name="baseIndex"></param>
		/// <param name="neighborInd"></param>
		/// <returns></returns>
		bool CheckIndexInRow(int baseIndex, int neighborInd)
        {
			int res = baseIndex - neighborInd;
			if (res == 1 || res == -1) // Если соседний элемент находится слева или справа, ...
            {
				int start = 0, end = 4;
				for (int i = 0; i < 5; ++i)
				{
					if ((baseIndex >= start && baseIndex <= end) && (neighborInd >= start && neighborInd <= end)) // ...то элементы должны принадлежать одному ряду.
					{
						return true;
					}
					start += 5;
					end += 5;
				}
				return false;
			}
			return true;
        }
	}
}