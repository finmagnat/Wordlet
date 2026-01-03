using System.Collections;
using Core.Audio;
using Core.Config;
using Core.Data;
using Core.Dictionary;
using Core.Events;
using Core.Generated;
using Core.Services;
using Core.UI;
using Cysharp.Threading.Tasks;
using Game.AI;
using UI.Popups;
using UI.Screens;
using UnityEngine;
using Zenject;

namespace Game.Logic
{
    /*
     * Управляет состояниями: инициализация поля, PlayerTurn, OpponentTurn, GameOver.
     */
    public class GameController
    {
        [Inject] private LocalizationService _localization;
        [Inject] private DictionaryService _dictionaryService;
        [Inject] private ConfigService _configService;
        [Inject] private AudioService _audioService;
        [Inject] private IUIManager _ui;
        
        private WordsFieldManager _wordsFieldManager = new ();
        private LettersFieldManager _lettersFieldManager = new ();
        private AIGameController _aIAlgorithm = new ();
        private GameScreen _gameScreen;
        private GameOpponent _gameOpponent;
        
        private bool _bStart; // Игра стартовала
        private bool _bPause; // Игра на паузе
        private bool _bLetterPut; // Буква установлена игроком (текущего клиента)
        private bool _bModePlayOwner = true; // Режим хода игрока (текущего клиента)
        private uint _maxPasses;
        private ComplexityAI _complexityAI;
        private int _durationGame;
        private string _firstWord;
        private SaveGameData _saveGameData;

        public async UniTask InitializeAsync()
        {
            EventBus.Subscribe<GameScreenStartEvent>(OnGameScreenStartEvent);
            
            EventBus.Subscribe<GamePauseEvent>(OnGamePause);
            EventBus.Subscribe<GameGoEvent>(OnGameGo);
            EventBus.Subscribe<GameClearEvent>(OnGameClear);
            EventBus.Subscribe<GameCancelEvent>(OnGameCancel);

            EventBus.Subscribe<LetterPutSuccessEvent>(OnLetterPutSuccess);
            EventBus.Subscribe<LetterPutToWordEvent>(OnLetterPutToWord);
            EventBus.Subscribe<PlayerErrorEvent>(OnErrorPlayer);

            EventBus.Subscribe<TimeExpiredEvent>(OnTimeExpired);

            EventBus.Subscribe<OpponentFindWordEvent>(OnOpponentFindWordSuccess);
            EventBus.Subscribe<OpponentFindWordFailEvent>(OnOpponentFindWordFail);
            
            _wordsFieldManager.Initialize();
            _lettersFieldManager.Initialize();
        }
        
        public void Destroy()
        {
            EventBus.Unsubscribe<GameScreenStartEvent>(OnGameScreenStartEvent);
            
            EventBus.Unsubscribe<GamePauseEvent>(OnGamePause);
            EventBus.Unsubscribe<GameGoEvent>(OnGameGo);
            EventBus.Unsubscribe<GameClearEvent>(OnGameClear);
            EventBus.Unsubscribe<GameCancelEvent>(OnGameCancel);

            EventBus.Unsubscribe<LetterPutSuccessEvent>(OnLetterPutSuccess);
            EventBus.Unsubscribe<LetterPutToWordEvent>(OnLetterPutToWord);
            EventBus.Unsubscribe<PlayerErrorEvent>(OnErrorPlayer);

            EventBus.Unsubscribe<TimeExpiredEvent>(OnTimeExpired);

            EventBus.Unsubscribe<OpponentFindWordEvent>(OnOpponentFindWordSuccess);
            EventBus.Unsubscribe<OpponentFindWordFailEvent>(OnOpponentFindWordFail);
            
            _wordsFieldManager.Destroy();
            _lettersFieldManager.Destroy();
        }

        public SaveGameData GetGameData()
        {
            var data = new SaveGameData
            {
                version = _configService.Game.version,
                localeCode = _localization.CurrentLocale.Identifier.Code,
                savedAtUtcTicks = System.DateTime.UtcNow.Ticks,
                
                mode = _gameOpponent.ToString(),
                boardSize = _configService.Game.defaultBoardSize,
                boardRows = _wordsFieldManager.WordsFieldData.GetBoardData(),
                levelComplexityAI = (int)_complexityAI,
                playerTurn = _bModePlayOwner,
                maxSeconds = _durationGame,
                currentSeconds = _gameScreen.TimerBar.GetCurrentValue(),
                
                playerScore = _gameScreen.PlayerPanelOwner.Score,
                opponentScore = _gameScreen.PlayerPanelOpponent.Score,
                playerPasses = _gameScreen.PlayerPanelOwner.Pass,
                opponentPasses = _gameScreen.PlayerPanelOpponent.Pass,
                
                firstWord = _firstWord,
                playerWords = _gameScreen.PlayerPanelOwner.Words,
                opponentWords = _gameScreen.PlayerPanelOpponent.Words
            };
            
            return data;
        }

        public void SetGameData(SaveGameData data)
        {
            _saveGameData = data;
        }
        
        private void OnGameScreenStartEvent(GameScreenStartEvent eventData)
        {   
            _gameScreen = eventData.Screen;
            _gameOpponent = eventData.Opponent;

            _gameScreen.PlayerPanelOwner.SetPlayerName(_localization.Get(LocalizationConst.TableUI,"NAME_PLAYER_OWNER")); // TODO: установить имя из профиля
            
            switch (_gameOpponent)
            {
                case GameOpponent.AI:
                    _gameScreen.PlayerPanelOpponent.SetPlayerName(_localization.Get(LocalizationConst.TableUI,"NAME_PLAYER_AI"));
                    
                    _complexityAI = _saveGameData != null ? 
                        (ComplexityAI)_saveGameData.levelComplexityAI :
                        (ComplexityAI)PlayerPrefs.GetInt(PlayerPrefsKey.ComplexityAI);
                    
                    var settings = _configService.Game.GetComplexityAIItem(_complexityAI);
                    _maxPasses = settings.MaxPasses;
                    
                    _aIAlgorithm.Init(_wordsFieldManager, _dictionaryService, settings);
                    
                    _bModePlayOwner = true;
                    break;
                case GameOpponent.FRIEND:
                    _gameScreen.PlayerPanelOpponent.SetPlayerName(_localization.Get(LocalizationConst.TableUI,"NAME_PLAYER_OPPONENT"));
                    _maxPasses = _configService.Game.maxPassesByDefault;
                    _bModePlayOwner = true; //TODO: в сетевой игре должен определять сервер, кто ходит первым
                    break;
            }
            
            var wordsFieldItems = _gameScreen.InitWordsField();
            _wordsFieldManager.SetWordsFieldData(wordsFieldItems);
                
            var lettersFieldItems = _gameScreen.InitAlphabetField();
            _lettersFieldManager.Init(lettersFieldItems);
            
            if(_saveGameData != null)
            {
                _firstWord = _saveGameData.firstWord;
                _wordsFieldManager.WordsFieldData.SetSaveGameData(_saveGameData);
                
                _gameScreen.PlayerPanelOwner.AddWords(_saveGameData.playerWords);
                _gameScreen.PlayerPanelOwner.SetData(_saveGameData.playerScore, _saveGameData.playerPasses, _maxPasses);
            
                _gameScreen.PlayerPanelOpponent.AddWords(_saveGameData.opponentWords);
                _gameScreen.PlayerPanelOpponent.SetData(_saveGameData.opponentScore, _saveGameData.opponentPasses, _maxPasses);

                _durationGame = _saveGameData.maxSeconds;
                _gameScreen.TimerBar.SetTargetValue(_durationGame, true);
                _gameScreen.TimerBar.SetCurrentValue(_saveGameData.currentSeconds);
            }
            else
            {
                _firstWord = _dictionaryService.GetRandomWord(_configService.Game.defaultBoardSize);
                _wordsFieldManager.SetFirstWord(_firstWord);
                
                _gameScreen.PlayerPanelOwner.SetPass(0, _maxPasses);
                _gameScreen.PlayerPanelOpponent.SetPass(0, _maxPasses);
                
                _durationGame = PlayerPrefs.GetInt(PlayerPrefsKey.DurationGame);
                _gameScreen.TimerBar.SetTargetValue(_durationGame, true);
            }
            
            _lettersFieldManager.SetEnable();

            if (_bModePlayOwner)
                _gameScreen.SetStatusLocalizationKey("STATUS_LABEL_GO_OWNER");
            else
                _gameScreen.SetStatusLocalizationKey("STATUS_LABEL_GO_OPPONENT");

            _saveGameData = null;
            _bStart = true;
            
            _audioService?.PlaySfxAsync(Sounds.SoundSfx_StartNewGame);
        }

        private void OnGamePause(IGameEvent eventData)
        {
            if (!_bStart || !_bModePlayOwner)
                return;

            _bPause = !_bPause;

            if (_bPause)
            {
                _gameScreen.SetStatusLocalizationKey("STATUS_LABEL_PAUSE");
                _gameScreen.TimerBar.StopTimer();
            }
            else
            {
                _gameScreen.TimerBar.StartTimer();
                _gameScreen.SetStatusLocalizationKey("STATUS_LABEL_GO_OWNER");
            }
            
            _lettersFieldManager.SetEnable(!_bPause);
            _wordsFieldManager.ShowLetters(!_bPause);
            
            _audioService?.PlaySfxAsync(Sounds.SoundSfx_Pause);
        }

        private async void OnGameGo(IGameEvent eventData)
        {
            if (!_bStart || _bPause || !_bModePlayOwner)
                return;

            if (!_bLetterPut)
            {                
                EventBus.Raise(new PlayerErrorEvent { GameError = GameError.NO_SETTED_LETTER });
                return;
            }

            string word = _gameScreen.GetTextWord();
            if (_wordsFieldManager.CheckWord(word))
            {                
                if (!_dictionaryService.Contains(word))
                {
                    _audioService?.PlaySfxAsync(Sounds.SoundSfx_PopupQuestion);
                    
                    var popup = await _ui.ShowPopupAsync<NewWordPopup>(AssetKey.NewWordPopup);
                    popup.SetWindowData(word);
                    
                    var dataResult = await popup.WaitForResultAsync();
                    
                    if(dataResult.Result == PopupResult.SaveAndExit)
                        SaveWordAndContinueGame(word);
                    else
                        Cancel();
                }
                else
                {
                    SaveWordAndContinueGame(word);
                    _audioService?.PlaySfxAsync(Sounds.SoundSfx_IMadeMove);
                }                
            }            
        }

        private void OnGameClear(IGameEvent eventData)
        {
            if (!_bStart || _bPause || !_bLetterPut || !_bModePlayOwner)
                return;
            
            _wordsFieldManager.Clear();
            _gameScreen.SetTextWord("");
        }

        private void OnGameCancel(IGameEvent eventData)
        {
            if (!_bStart || _bPause || !_bLetterPut || !_bModePlayOwner)
                return;
            
            Cancel();
        }

        private void Cancel()
        {
            _bLetterPut = false;
            _wordsFieldManager.Cancel();
            _gameScreen.SetTextWord("");
            _lettersFieldManager.SetEnable();
        }

        private void OnLetterPutSuccess(IGameEvent eventData)
        {
            _bLetterPut = true;
            _lettersFieldManager.SetEnable(false);
            _audioService?.PlaySfxAsync(Sounds.SoundSfx_LetterPutSuccess);
        }

        private void OnLetterPutToWord(LetterPutToWordEvent eventData)
        {
            _gameScreen.AddLetterToWord(eventData.letter);
            _audioService?.PlaySfxAsync(Sounds.SoundSfx_LetterSelected);
        }

        private async void OnErrorPlayer(PlayerErrorEvent eventData)
        {
            Debug.Log("[GameController] [OnErrorPlayer] " + eventData.GameError);
           
            var messageBoxData = new MessageBoxData
            {
                Error = eventData.GameError
            };

            if (eventData.GameError == GameError.SET_LETTER_NO_SELECTED)
                messageBoxData.ExecuteOnClose = () => { _wordsFieldManager.BlinkNoSelectedLetter(); };

            var popup = await _ui.ShowPopupAsync<AdvicePopup>(AssetKey.AdvicePopup);
            popup.SetWindowData(messageBoxData);
            
            _audioService?.PlaySfxAsync(Sounds.SoundSfx_PopupWorning);
        }
        
        private void SaveWordAndContinueGame(string word)
        {
            _gameScreen.TimerBar.ResetTimer();
            if (_bModePlayOwner)
            {
                _gameScreen.PlayerPanelOwner.AddWord(word);                
                _gameScreen.PlayerPanelOwner.SetScore(_gameScreen.PlayerPanelOwner.Score + (uint)word.Length);
            }
            else
            {
                _gameScreen.PlayerPanelOpponent.AddWord(word);
                _gameScreen.PlayerPanelOpponent.SetScore(_gameScreen.PlayerPanelOpponent.Score + (uint)word.Length);
            }
            
            _gameScreen.SetTextWord("");

            // TODO: Добавить новое слово в локальный словарь игрока
            //_dictionaryService.AddWord(word);
            
            _wordsFieldManager.SaveWord(word);
            _wordsFieldManager.Clear();
            
            _bLetterPut = false;

            CheckFinishGame();
        }

        private void OnTimeExpired(IGameEvent eventData)
        {
            _gameScreen.TimerBar.ResetTimer();

            if (_bLetterPut)
                Cancel();

            if (_bModePlayOwner)
                _gameScreen.PlayerPanelOwner.SetPass(_gameScreen.PlayerPanelOwner.Pass + 1, _maxPasses);
            else
                _gameScreen.PlayerPanelOpponent.SetPass(_gameScreen.PlayerPanelOpponent.Pass + 1, _maxPasses);
            
            _audioService?.PlaySfxAsync(Sounds.SoundSfx_Pass);
            
            CheckFinishGame();
        }

        private void OnOpponentFindWordSuccess(OpponentFindWordEvent eventData)
        {
            _gameScreen.TimerBar.StopTimer();
            _gameScreen.SetTextWord(eventData.word);
            _gameScreen.StartCoroutine(DisplayWordOpponent(eventData));
            
            _audioService?.PlaySfxAsync(Sounds.SoundSfx_OpponentMadeMove);
        }

        private IEnumerator DisplayWordOpponent(OpponentFindWordEvent eventData)
        {
            yield return new WaitForSeconds(2);
            SaveWordAndContinueGame(eventData.word);
        }

        private void OnOpponentFindWordFail(IGameEvent eventData)
        {
            _gameScreen.PlayerPanelOpponent.SetPass(_gameScreen.PlayerPanelOpponent.Pass + 1, _maxPasses);
            _audioService?.PlaySfxAsync(Sounds.SoundSfx_OpponentFindWordFail);
            CheckFinishGame();
        }

        private void CheckFinishGame()
        {
            // Допустил ли кто-то максимальное количество пропусков?
            // Заполнено ли полностью поле?
            if (_gameScreen.PlayerPanelOwner.Pass >= _maxPasses ||
                 _gameScreen.PlayerPanelOpponent.Pass >= _maxPasses ||
                _wordsFieldManager.Filled())            
            {
                FinishGame();
            }
            else
            {
                SwitchPlayer(); // Переход хода
            }
        }

        private void SwitchPlayer()
        {
            _bModePlayOwner = !_bModePlayOwner;
            _gameScreen.TimerBar.StartTimer();
            if (_bModePlayOwner)
            {
                _gameScreen.SetStatusLocalizationKey("STATUS_LABEL_GO_OWNER");
                _lettersFieldManager.SetEnable();
            }
            else
            {
                _gameScreen.SetStatusLocalizationKey("STATUS_LABEL_GO_OPPONENT");
                _lettersFieldManager.SetEnable(false);
                _wordsFieldManager.SetModeSelect(false);
                switch (_gameOpponent)
                {
                    case GameOpponent.AI:
                        _aIAlgorithm.PlayAsync();
                        break;
                }
            }
        }

        private void FinishGame()
        {
            _bStart = false;
            _bPause = false;
            _bLetterPut = false;

            // Определение победителя:
            ResultGame resultGame = ResultGame.DRAW;
            bool bResultDetermined = false;

            // По пропускам
            if (_gameScreen.PlayerPanelOwner.Pass >= _maxPasses)
            {
                bResultDetermined = true;
                resultGame = ResultGame.OWNER_LOSE;
            }
            else if(_gameScreen.PlayerPanelOpponent.Pass >= _maxPasses)
            {
                bResultDetermined = true;
                resultGame = ResultGame.OWNER_WIN;
            }
                        
            if (!bResultDetermined)
            {
                // По очкам
                if (_gameScreen.PlayerPanelOwner.Score > _gameScreen.PlayerPanelOpponent.Score)
                    resultGame = ResultGame.OWNER_WIN;
                else if (_gameScreen.PlayerPanelOwner.Score < _gameScreen.PlayerPanelOpponent.Score)
                    resultGame = ResultGame.OWNER_LOSE;
            }

            EventBus.Raise(new GameEndEvent());
            switch (resultGame)
            {
                case ResultGame.OWNER_WIN:
                    _ui.ShowPopupAsync<MessagePopup>(AssetKey.WinPopup);
                    _audioService?.PlaySfxAsync(Sounds.SoundSfx_IWon);
                    break;
                case ResultGame.OWNER_LOSE:
                    _ui.ShowPopupAsync<MessagePopup>(AssetKey.LosePopup);
                    _audioService?.PlaySfxAsync(Sounds.SoundSfx_OpponentWon);
                    break;
                default:
                    _ui.ShowPopupAsync<MessagePopup>(AssetKey.DrawPopup);
                    _audioService?.PlaySfxAsync(Sounds.SoundSfx_Draw);
                    break;
            }
        }

        private void Reset()
        {
            _wordsFieldManager.Reset();
            _gameScreen.Reset();
        }
        
    }
}