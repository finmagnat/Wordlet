using System;
using System.Collections;
using System.Threading;
using Core.Audio;
using Core.Config;
using Core.Data;
using Core.DataDictionary;
using Core.Events;
using Core.Generated;
using Core.Services;
using Core.UI;
using Cysharp.Threading.Tasks;
using Game.AI;
using Inventory;
using UI.Popups;
using UI.Screens;
using UnityEngine;
using Zenject;

namespace Game.Logic
{
    /*
     * Управляет состояниями: инициализация поля, PlayerTurn, OpponentTurn, GameOver.
     */
    public class GameController : IDisposable
    {
        [Inject] private DictionaryService _dictionaryService;
        [Inject] private LocalizationService _localization;
        [Inject] private IInventoryService _inventory;
        [Inject] private InventorySyncService _inventorySync;
        [Inject] private ConfigService _configService;
        [Inject] private IProfileService _profile;
        [Inject] private AudioService _audioService;
        [Inject] private IUIManager _ui;
        
        private readonly SemaphoreSlim _blockUiLock = new(1, 1);
        
        private WordsFieldManager _wordsFieldManager = new ();
        private LettersFieldManager _lettersFieldManager = new ();
        private AIGameController _ai = new ();
        private GameScreen _gameScreen;
        private GameOpponent _gameOpponent;
        
        private bool _bStart; // Игра стартовала
        private bool _bPause; // Игра на паузе
        private bool _bLetterPut; // Буква установлена игроком (текущего клиента)
        private bool _bModePlayOwner = true; // Режим хода игрока (текущего клиента)
        private uint _maxPasses;
        private ComplexityAI _complexityAI;
        private ComplexityAISettings _complexityAISettings;
        private int _durationGame;
        private string _firstWord;
        private SaveGameData _saveGameData;
        private bool _boosterProcessing;

        public async UniTask InitializeAsync()
        {
            EventBus.Subscribe<GameScreenStartEvent>(OnGameScreenStartEvent);
            
            EventBus.Subscribe<GamePauseChangedEvent>(OnGamePause);
            EventBus.Subscribe<GameGoEvent>(OnGameGo);
            EventBus.Subscribe<GameClearEvent>(OnGameClear);
            EventBus.Subscribe<GameCancelEvent>(OnGameCancel);
            EventBus.Subscribe<GameSkipEvent>(OnGameSkip);

            EventBus.Subscribe<LetterPutSuccessEvent>(OnLetterPutSuccess);
            EventBus.Subscribe<LetterPutToWordEvent>(OnLetterPutToWord);
            EventBus.Subscribe<LetterRemoveLastFromWordEvent>(OnLetterRemoveLastFromWord);
            EventBus.Subscribe<PlayerErrorEvent>(OnErrorPlayer);

            EventBus.Subscribe<TimeExpiredEvent>(OnTimeExpired);

            EventBus.Subscribe<OpponentFindWordEvent>(OnOpponentFindWordSuccess);
            EventBus.Subscribe<OpponentFindWordFailEvent>(OnOpponentFindWordFail);
            
            EventBus.Subscribe<UseBoosterEvent>(OnActivateBooster);
            
            _wordsFieldManager.Initialize();
            _lettersFieldManager.Initialize();
        }
        
        public void Dispose()
        {
            EventBus.Unsubscribe<GameScreenStartEvent>(OnGameScreenStartEvent);
            
            EventBus.Unsubscribe<GamePauseChangedEvent>(OnGamePause);
            EventBus.Unsubscribe<GameGoEvent>(OnGameGo);
            EventBus.Unsubscribe<GameClearEvent>(OnGameClear);
            EventBus.Unsubscribe<GameCancelEvent>(OnGameCancel);
            EventBus.Unsubscribe<GameSkipEvent>(OnGameSkip);

            EventBus.Unsubscribe<LetterPutSuccessEvent>(OnLetterPutSuccess);
            EventBus.Unsubscribe<LetterPutToWordEvent>(OnLetterPutToWord);
            EventBus.Unsubscribe<LetterRemoveLastFromWordEvent>(OnLetterRemoveLastFromWord);
            EventBus.Unsubscribe<PlayerErrorEvent>(OnErrorPlayer);

            EventBus.Unsubscribe<TimeExpiredEvent>(OnTimeExpired);

            EventBus.Unsubscribe<OpponentFindWordEvent>(OnOpponentFindWordSuccess);
            EventBus.Unsubscribe<OpponentFindWordFailEvent>(OnOpponentFindWordFail);
            
            EventBus.Unsubscribe<UseBoosterEvent>(OnActivateBooster);
            
            _wordsFieldManager.Destroy();
            _lettersFieldManager.Destroy();
        }
        
        public SaveGameData GetGameData()
        {
            if (_bLetterPut)
                Cancel();
            
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
            _gameScreen.BoosterPanel.Refresh();
            _wordsFieldManager.Reset();
            
            _gameScreen.PlayerPanelOwner.SetPlayerName(_localization.Get(LocalizationConst.TableUI,"NAME_PLAYER_OWNER")); // TODO: установить имя из профиля
            
            switch (_gameOpponent)
            {
                case GameOpponent.AI:
                    _gameScreen.PlayerPanelOpponent.SetPlayerName(_localization.Get(LocalizationConst.TableUI,"NAME_PLAYER_AI"));
                    
                    _complexityAI = _saveGameData != null ? 
                        (ComplexityAI)_saveGameData.levelComplexityAI :
                        (ComplexityAI)PlayerPrefs.GetInt(PlayerPrefsKey.ComplexityAI);
                    
                    _complexityAISettings = _configService.Game.GetComplexityAIItem(_complexityAI);
                    _maxPasses = _complexityAISettings.MaxPasses;
                    
                    _ai.Init(_wordsFieldManager, _dictionaryService);
                    
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

        /*private void OnGamePause(IGameEvent eventData)
        {
            if (!_bStart || !_bModePlayOwner || _gameScreen.BoosterPanel.IsActive(BoosterType.Slowdown))
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
        }*/
        
        private void OnGamePause(GamePauseChangedEvent eventData)
        {
            //var e = (GamePauseChangedEvent)eventData;

            if (!_bStart || !_bModePlayOwner || _gameScreen.BoosterPanel.IsActive(BoosterType.Slowdown))
                return;

            _bPause = eventData.IsPaused;

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
                    
                    var popup = await _ui.ShowPopupAsync<MissingWordPopup>(AssetKey.MissingWordPopup);
                    popup.SetWindowData(word);
                    
                    var dataResult = await popup.WaitForResultAsync();
                    /*
                    // Функция сохранения новых слов пока отключена по этическим причинам.
                    if(dataResult.Result == PopupResult.SaveAndExit)
                        SaveWordAndContinueGame(word);
                    else
                    */
                    Cancel();
                }
                else
                {
                    SaveWordAndContinueGame(word);
                    _audioService?.PlaySfxAsync(Sounds.SoundSfx_IMadeMove);
                    _gameScreen.BoosterPanel.SlowdownStop();
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

        private void OnLetterRemoveLastFromWord(LetterRemoveLastFromWordEvent eventData)
        {
            // удалить последнюю букву
            _gameScreen.RemoveLastLetter();
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

            // TODO: Добавить новое слово в локальный словарь игрока [пока это не делаем]
            //_dictionaryService.AddWord(word);
            
            _wordsFieldManager.SaveWord(word);
            _wordsFieldManager.Clear();
            
            _bLetterPut = false;

            CheckFinishGame();
        }

        private void OnGameSkip(IGameEvent eventData)
        {
            if (!_bStart || _bPause || !_bModePlayOwner)
                return;
            
            _gameScreen.BoosterPanel.SlowdownStop();
            PassedGame();
        }
        
        private void OnTimeExpired(IGameEvent eventData)
        {
            _ai.AbortSearch();
            
            PassedGame();
        }
        
        private void PassedGame()
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
                        AIPlayAsync();
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
            
            _gameScreen.BoosterPanel.SlowdownStop();
            
            ShowFinishGamePopup(resultGame);
        }
        
        protected async void ShowFinishGamePopup(ResultGame resultGame)
        {
            // TEST
            //resultGame = ResultGame.DRAW;
            //*******************************

            _profile.AddScoreAsync((int)_gameScreen.PlayerPanelOwner.Score);
                
            FinishGamePopup finishPopup;
            switch (resultGame)
            {
                case ResultGame.OWNER_WIN:
                    finishPopup = await _ui.ShowPopupAsync<FinishGamePopup>(AssetKey.WinPopup);
                    _audioService?.PlaySfxAsync(Sounds.SoundSfx_IWon);
                    break;
                case ResultGame.OWNER_LOSE:
                    finishPopup = await _ui.ShowPopupAsync<FinishGamePopup>(AssetKey.LosePopup);
                    _audioService?.PlaySfxAsync(Sounds.SoundSfx_OpponentWon);
                    break;
                default:
                    finishPopup = await _ui.ShowPopupAsync<FinishGamePopup>(AssetKey.DrawPopup);
                    _audioService?.PlaySfxAsync(Sounds.SoundSfx_Draw);
                    break;
            }
            
            finishPopup.statsTable.SetData(
                _gameScreen.PlayerPanelOwner.PlayerName,
                _gameScreen.PlayerPanelOpponent.PlayerName,
                _gameScreen.PlayerPanelOwner.Score,
                _gameScreen.PlayerPanelOpponent.Score,
                _gameScreen.PlayerPanelOwner.Pass,
                _gameScreen.PlayerPanelOpponent.Pass,
                _maxPasses
                );
        }
        
        private async UniTaskVoid AIPlayAsync()
        {
            var res = await _ai.FindWordAsync(_complexityAISettings);

            if (res.Success)
                EventBus.Raise(new OpponentFindWordEvent { word = res.Word });
            else
                EventBus.Raise(new OpponentFindWordFailEvent());
        }
        
        private async void OnActivateBooster(UseBoosterEvent eventData)
        {
            if (_boosterProcessing)
                return;

            if (!_bStart || _bPause || !_bModePlayOwner)
                return;

            // не даём прожать активный бустер
            if (_gameScreen.BoosterPanel.IsActive(eventData.boosterType))
                return;

            _boosterProcessing = true;
            await BlockUIAsync(true);

            // 1) серверное списание
            // TryUseBoosterAsync должен:
            // - дернуть CloudScript ConsumeBooster
            // - обновить локальный IInventoryService из ответа/ресинка
            // - вернуть true/false
            bool ok = await _inventorySync.TryUseBoosterAsync(eventData.boosterType);
            
            // 2) обновляем UI бустеров после серверного результата
            _gameScreen.BoosterPanel.Refresh();

            if (!ok)
            {
                await BlockUIAsync(false);
                _boosterProcessing = false;
                return;
            }

            // 3) применяем эффект
            switch (eventData.boosterType)
            {
                case BoosterType.Letter:
                    await ActivateBoosterLetterAsync();   // см. ниже — убираем внутренний BlockUI
                    break;

                case BoosterType.Slowdown:
                    ActivateBoosterSlowdownAsync();       // можно оставить async void, он не критичен
                    break;
            }

            await BlockUIAsync(false);
            _boosterProcessing = false;
        }

        
        private async UniTask ActivateBoosterLetterAsync()
        {
            Cancel(); // "очистить мусор"

            var boosterSettings = _configService.Game.GetComplexityAIItem(ComplexityAI.HARD);
            var res = await _ai.FindWordAsync(boosterSettings);

            if (res.Success)
                ShowBoosterLetterSuccess(res.Word);
            else
                await ShowBoosterLetterFailAsync();
        }


        private void ShowBoosterLetterSuccess(string resWord)
        {
            _gameScreen.SetTextWord(resWord);
            _wordsFieldManager.SetModeSelect(true);
            _bLetterPut = true;
            _lettersFieldManager.SetEnable(false);
            _audioService?.PlaySfxAsync(Sounds.SoundSfx_LetterPutSuccess);
            _gameScreen.SetStatusLocalizationKey("STATUS_LABEL_BOOSTER_SUCCESS");
        }

        private async UniTask ShowBoosterLetterFailAsync()
        {
            _gameScreen.SetStatusLocalizationKey("STATUS_LABEL_BOOSTER_FAIL");

            // серверный возврат
            await _inventorySync.GrantBoosterAsync(BoosterType.Letter, 1);

            // и обновить панель
            _gameScreen.BoosterPanel.Refresh();
        }

        private async UniTask BlockUIAsync(bool isBlocked)
        {
            await _blockUiLock.WaitAsync();
            try
            {
                if (isBlocked)
                    await _ui.ShowScreenAsync<BlockUIScreen>(AssetKey.BlockUIScreen);
                else
                    await _ui.HideScreenAsync<BlockUIScreen>(AssetKey.BlockUIScreen);
            }
            finally
            {
                _blockUiLock.Release();
            }
        }

        private async void ActivateBoosterSlowdownAsync()
        {
            if (!_bStart || _bPause || !_bModePlayOwner)
                return;
            
            _gameScreen.TimerBar.StopTimer();
            _gameScreen.BoosterPanel.SlowdownStart();
            
            await UniTask.WaitForSeconds(_configService.Game.slowdownDelay);
            
            if (!_bPause && _bStart)
                _gameScreen.TimerBar.StartTimer();
        }
        
    }
}