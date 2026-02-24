using System.Collections.Generic;
using Core.Config;
using Core.Data;
using Core.Events;
using UI.Components;

namespace Game.Logic
{
    public class WordsFieldManager
    {
        public WordsFieldData WordsFieldData => _wordsFildData;
        
        private WordsFieldData _wordsFildData = new ();
        
        private bool _bModeSelectWord;

        public void Initialize()
        {
            EventBus.Subscribe<LetterReleaseEvent>(OnLetterRelease);
            EventBus.Subscribe<LetterSelectEvent>(OnLetterSelected);
            EventBus.Subscribe<GameEndEvent>(OnGameEnd);
            EventBus.Subscribe<LetterBacktrackEvent>(OnLetterBacktrack);
        }

        public void Destroy()
        {
            EventBus.Unsubscribe<LetterReleaseEvent>(OnLetterRelease);
            EventBus.Unsubscribe<LetterSelectEvent>(OnLetterSelected);
            EventBus.Unsubscribe<GameEndEvent>(OnGameEnd);
            EventBus.Unsubscribe<LetterBacktrackEvent>(OnLetterBacktrack);
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
            if (!_bModeSelectWord) return;
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
            _bModeSelectWord = false;
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
        
        private void OnLetterRelease(LetterReleaseEvent eventData)
        {
            //Debug.Log("[OnLetterReleaseEvent] Position: " + data.position + ", Letter: " + data.letter);

            if(_wordsFildData.CheckSetLetter(eventData.position, eventData.letter))
            {
                EventBus.Raise(new LetterPutSuccessEvent());
                _bModeSelectWord = true;
            }
        }

        private void OnLetterSelected(LetterSelectEvent eventData)
        {
            if (!_bModeSelectWord) return;

            //Debug.Log("[LetterSelectEventData] Index: " + data.item.Index + ", Letter: " + data.item.GetLetter());

            if (_wordsFildData.CheckSelectLetter(eventData.letter))
            {
                eventData.letter.Highlight();
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
    }
}