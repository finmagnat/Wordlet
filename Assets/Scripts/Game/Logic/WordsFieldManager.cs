using System.Collections.Generic;
using Core.Config;
using Core.Data;
using Core.Events;
using UI.Components;
using UnityEngine;

namespace Game.Logic
{
    public class WordsFieldManager
    {
        public WordsFieldData WordsFieldData => _wordsFildData;
        
        private WordsFieldData _wordsFildData = new ();
        
        private bool _bModeSelectWord;
        private bool _isPause;
        private bool _bModeEraser;

        public void Initialize()
        {
            EventBus.Subscribe<KeyboardLetterSelectEvent>(OnKeyboardLetterSelect);
            EventBus.Subscribe<LetterSelectEvent>(OnLetterSelected);
            EventBus.Subscribe<GameEndEvent>(OnGameEnd);
            EventBus.Subscribe<LetterBacktrackEvent>(OnLetterBacktrack);
            EventBus.Subscribe<GamePauseChangedEvent>(OnGamePause);
        }

        public void Destroy()
        {
            EventBus.Unsubscribe<KeyboardLetterSelectEvent>(OnKeyboardLetterSelect);
            EventBus.Unsubscribe<LetterSelectEvent>(OnLetterSelected);
            EventBus.Unsubscribe<GameEndEvent>(OnGameEnd);
            EventBus.Unsubscribe<LetterBacktrackEvent>(OnLetterBacktrack);
            EventBus.Unsubscribe<GamePauseChangedEvent>(OnGamePause);
        }

        internal void SetWordsFieldData(List<SelectableLetter> items)
        {
            _wordsFildData.SetItems(items);
        }
        
        internal void SetFirstWord(string word)
        {
            _wordsFildData.SetFirstWord(word);
        }

        internal void Clear()
        {
            _wordsFildData.Clear();
        }

        internal void Cancel()
        {
            _bModeSelectWord = false;
            _wordsFildData.Cancel();
        }

        /// <summary>
        /// Проверка условий сборки слова
        /// </summary>
        /// <param name="word">Проверяемое слово</param>
        /// <returns>Успех</returns>
        internal bool CheckWord(string word)
        {
            if (!_bModeSelectWord) return false;
            GameError err = _wordsFildData.CheckWord(word);
            if (err == GameError.NONE)
                return true;
            
            EventBus.Raise(new PlayerErrorEvent { GameError = err });
            return false;
        }

        internal void BlinkNoSelectedLetter()
        {
            _wordsFildData.BlinkNoSelectedLetter();
        }

        internal void SaveWord(string word)
        {
            _wordsFildData.SaveWord(word);
        }

        internal void ShowLetters(bool value)
        {
            _wordsFildData.ShowLetters(value);
        }

        internal void Reset()
        {
            _isPause = false;
            _bModeSelectWord = false;
            _bModeEraser = false;
            _wordsFildData.Reset();
        }
        
        internal void SetModeSelect(bool value)
        {
            _bModeSelectWord = value;
        }

        internal bool Filled()
        {
            return _wordsFildData.Filled();
        }

        internal bool WordExist(string word)
        {
            return _wordsFildData.Exist(word);
        }
        
        internal bool TrySetLetter(int indexItem)
        {
            return _wordsFildData.TrySetLetter(indexItem);
        }
        
        /// <summary>
        /// Проверяет можно ли выбрать данную пустую ячейку, чтобы позже установить в нее букву.
        /// Если выделенная ячейка повторно нажата, то выделение отменяется.
        /// Если выделена еще одна (другая) ячейка, то выделение переносится на последнюю из них.
        /// </summary>
        /// <param name="eventData"></param>
        internal void TryCellSelect(CellSelectEvent eventData)
        {
            if (_bModeSelectWord || _bModeEraser) return;
            
            if (_wordsFildData.SelectedItem != null && 
                _wordsFildData.SelectedItem.Index == eventData.letter.Index) // Эта ячейка уже была выделена
            {
                _wordsFildData.CellSelectCancel();
                //Debug.Log("[WordsFieldManager][TryCellSelect] [CellSelect Cancel Event] Position: " + eventData.letter.Index + ", Letter: " + eventData.letter.GetLetter());
                EventBus.Raise(new CellSelectCancelEvent());
                return;
            }
            
            if(_wordsFildData.TrySetLetter(eventData.letter.Index))
            {
                if (_wordsFildData.SelectedItem != null) // Другая ячейка уже была выделена
                    _wordsFildData.CellSelectCancel();
                
                _wordsFildData.SetSelectedCell(eventData.letter);
                eventData.letter.HighlightCell();
                //Debug.Log("[WordsFieldManager][TryCellSelect] [CellSelect Success Event] Position: " + eventData.letter.Index + ", Letter: " + eventData.letter.GetLetter());
                EventBus.Raise(new CellSelectSuccessEvent());
            }
        }
        
        internal void SetModeEraser(bool bModeEraser)
        {
            _bModeEraser = bModeEraser;
        }

        private void OnKeyboardLetterSelect(KeyboardLetterSelectEvent eventData)
        {
            if(_wordsFildData.SetLetterToSelectedCell(eventData.letter))
            {
                EventBus.Raise(new LetterPutSuccessEvent());
                _bModeSelectWord = true;
            }
        }

        private void OnLetterSelected(LetterSelectEvent eventData)
        {
            if (_isPause) 
                return;
            
            if (_bModeEraser && !eventData.letter.Empty())
            {
                eventData.letter.SetLetter("");
                _wordsFildData.SetSelectedCell(eventData.letter);
                eventData.letter.HighlightCell();
                //Debug.Log("[WordsFieldManager][OnLetterSelected] [CellSelect Success Event] Index: " + eventData.letter.Index + ", Letter: " + eventData.letter.GetLetter());
                EventBus.Raise(new CellSelectSuccessEvent { letter = eventData.letter });
                return;
            }
            
            if (_bModeSelectWord && _wordsFildData.CheckSelectLetter(eventData.letter))
            {
                eventData.letter.Highlight();
                //Debug.Log("[WordsFieldManager][OnLetterSelected] [Letter Put To Word Event] Index: " + eventData.letter.Index + ", Letter: " + eventData.letter.GetLetter());
                EventBus.Raise(new LetterPutToWordEvent { letter = eventData.letter.GetLetter() });                
            }
        }

        private void OnGameEnd(GameEndEvent eventData)
        {
            SetModeSelect(false);
        }

        private void OnLetterBacktrack(LetterBacktrackEvent eventData)
        {
            if (!_bModeSelectWord) return;

            if (_wordsFildData.BacktrackOneStep(out var removedItem))
            {
                removedItem.UnHighlight();
                EventBus.Raise(new LetterRemoveLastFromWordEvent());
            }
        }

        private void OnGamePause(GamePauseChangedEvent eventData)
        {
            _isPause = eventData.IsPaused;
        }
    }
}