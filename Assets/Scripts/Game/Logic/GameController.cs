using System;
using System.Collections;
using System.Threading;
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
        [Inject] private MissingWordPopupPresenter _missingWordPopupPresenter;
        [Inject] private InterstitialPolicyService _interstitialService;
        
        private readonly SemaphoreSlim _blockUiLock = new(1, 1);
        
        private WordsFieldManager _wordsFieldManager = new ();
        private AIGameController _ai = new ();
        private GameScreenBase _gameScreen;
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
        private SaveGameData _saveGameDataRepeat;
        private bool _boosterProcessing;
        private bool _pauseCooldown;

        public async UniTask InitializeAsync()
        {
            EventBus.Subscribe<GameScreenStartEvent>(OnGameScreenStartEvent);
            
            EventBus.Subscribe<GamePauseChangedEvent>(OnGamePause);
            EventBus.Subscribe<GameGoEvent>(OnGameGo);
            EventBus.Subscribe<GameCancelEvent>(OnGameCancel);
            EventBus.Subscribe<GameSkipEvent>(OnGameSkip);
            EventBus.Subscribe<RepeatGameEvent>(OnRepeatGame);

            EventBus.Subscribe<CellSelectEvent>(OnCellSelect);
            EventBus.Subscribe<CellSelectSuccessEvent>(OnCellSelectSuccess);
            EventBus.Subscribe<LetterPutSuccessEvent>(OnLetterPutSuccess);
            EventBus.Subscribe<LetterPutToWordEvent>(OnLetterPutToWord);
            EventBus.Subscribe<LetterRemoveLastFromWordEvent>(OnLetterRemoveLastFromWord);
            EventBus.Subscribe<PlayerErrorEvent>(OnErrorPlayer);
            EventBus.Subscribe<ModeBlinkEndEvent>(OnModeBlinkEnd); 

            EventBus.Subscribe<TimeExpiredEvent>(OnTimeExpired);

            EventBus.Subscribe<OpponentFindWordEvent>(OnOpponentFindWordSuccess);
            EventBus.Subscribe<OpponentFindWordFailEvent>(OnOpponentFindWordFail);
            
            EventBus.Subscribe<UseBoosterEvent>(OnActivateBooster);
            
            _wordsFieldManager.Initialize();
        }
        
        public void Dispose()
        {
            EventBus.Unsubscribe<GameScreenStartEvent>(OnGameScreenStartEvent);
            
            EventBus.Unsubscribe<GamePauseChangedEvent>(OnGamePause);
            EventBus.Unsubscribe<GameGoEvent>(OnGameGo);
            EventBus.Unsubscribe<GameCancelEvent>(OnGameCancel);
            EventBus.Unsubscribe<GameSkipEvent>(OnGameSkip);
            EventBus.Unsubscribe<RepeatGameEvent>(OnRepeatGame);

            EventBus.Unsubscribe<CellSelectEvent>(OnCellSelect);
            EventBus.Unsubscribe<CellSelectSuccessEvent>(OnCellSelectSuccess);
            EventBus.Unsubscribe<LetterPutSuccessEvent>(OnLetterPutSuccess);
            EventBus.Unsubscribe<LetterPutToWordEvent>(OnLetterPutToWord);
            EventBus.Unsubscribe<LetterRemoveLastFromWordEvent>(OnLetterRemoveLastFromWord);
            EventBus.Unsubscribe<PlayerErrorEvent>(OnErrorPlayer);
            EventBus.Unsubscribe<ModeBlinkEndEvent>(OnModeBlinkEnd); 

            EventBus.Unsubscribe<TimeExpiredEvent>(OnTimeExpired);

            EventBus.Unsubscribe<OpponentFindWordEvent>(OnOpponentFindWordSuccess);
            EventBus.Unsubscribe<OpponentFindWordFailEvent>(OnOpponentFindWordFail);
            
            EventBus.Unsubscribe<UseBoosterEvent>(OnActivateBooster);
            
            _wordsFieldManager.Destroy();
        }

        public SaveGameData GetGameData()
        {
            if (_bLetterPut)
                Cancel();
            
            var data = new SaveGameData
            {
                version = _configService.Game.version,
                localeCode = _localization.CurrentLocale.Identifier.Code,
                savedAtUtcTicks = DateTime.UtcNow.Ticks,
                
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
                playerWords = _gameScreen.StatisticsPanel.StatisticPlayerPlayerPanelOwner.Words,
                opponentWords = _gameScreen.StatisticsPanel.StatisticPlayerPlayerPanelOpponent.Words
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
            
            _gameScreen.GoButton.SetActive(false);
            _gameScreen.CancelButton.SetActive(false);
            _gameScreen.PauseButton.interactable = true;
            _gameScreen.PassButton.interactable = true;
            
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
                
            _gameScreen.InitAlphabetField();
            
            if(_saveGameData != null)
            {
                _firstWord = _saveGameData.firstWord;
                _wordsFieldManager.WordsFieldData.SetSaveGameData(_saveGameData);
                
                _gameScreen.StatisticsPanel.SetStartWord(_firstWord);
                _gameScreen.StatisticsPanel.StatisticPlayerPlayerPanelOwner.AddWords(_saveGameData.playerWords);
                _gameScreen.PlayerPanelOwner.SetData(_saveGameData.playerScore, _saveGameData.playerPasses, _maxPasses);
            
                _gameScreen.StatisticsPanel.StatisticPlayerPlayerPanelOpponent.AddWords(_saveGameData.opponentWords);
                _gameScreen.PlayerPanelOpponent.SetData(_saveGameData.opponentScore, _saveGameData.opponentPasses, _maxPasses);

                _durationGame = _saveGameData.maxSeconds;
                _gameScreen.TimerBar.SetTargetValue(_durationGame, true);
                _gameScreen.TimerBar.SetCurrentValue(_saveGameData.currentSeconds);
            }
            else
            {
                _firstWord = _dictionaryService.GetRandomWord(_configService.Game.defaultBoardSize);
                _wordsFieldManager.SetFirstWord(_firstWord);
                
                _gameScreen.StatisticsPanel.SetStartWord(_firstWord);
                _gameScreen.PlayerPanelOwner.SetPass(0, _maxPasses);
                _gameScreen.PlayerPanelOpponent.SetPass(0, _maxPasses);
                
                _durationGame = PlayerPrefs.GetInt(PlayerPrefsKey.DurationGame);
                _gameScreen.TimerBar.SetTargetValue(_durationGame, true);
            }

            if (_bModePlayOwner)
                _gameScreen.SetStatusLocalizationKey("STATUS_LABEL_GO_OWNER");
            else
                _gameScreen.SetStatusLocalizationKey("STATUS_LABEL_GO_OPPONENT");

            _saveGameDataRepeat = _saveGameData;
            _saveGameData = null;
            _bStart = true;
            
            _audioService?.PlaySfxAsync(AssetKey.sfx_start_new_game.ToString());
        }

        private void OnGamePause(GamePauseChangedEvent eventData)
        {
            if (!_bStart || !_bModePlayOwner || _gameScreen.BoosterPanel.IsActive(BoosterType.Slowdown))
                return;
            
            PauseCooldownAsync();
            
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

            _wordsFieldManager.ShowLetters(!_bPause);

            _audioService?.PlaySfxAsync(AssetKey.sfx_pause.ToString());
        }

        private async void PauseCooldownAsync()
        {
            _gameScreen.PauseButton.interactable = false;
            await UniTask.WaitForSeconds(_configService.Game.pauseCooldownSeconds);
            _gameScreen.PauseButton.interactable = true;
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
                    _audioService?.PlaySfxAsync(AssetKey.sfx_popup_question.ToString());

                    await _missingWordPopupPresenter.ShowAsync(
                        word,
                        _dictionaryService.DictionaryConfig.languageCode);
                    
                    Cancel(); // Сразу в словарь ничего не добавляем.
                }
                else
                {
                    SaveWordAndContinueGame(word);
                }                
            }            
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
            _gameScreen.GoButton.SetActive(false);
            _gameScreen.CancelButton.SetActive(false);
        }
        
        internal void OnCellSelect(CellSelectEvent eventData)
        {
            //Debug.Log("[CaneSelectCell] Position: " + data.position + ", Letter: " + data.letter);
            if (!_bStart || _bPause || !_bModePlayOwner)
                return;

            _wordsFieldManager.TryCellSelect(eventData);
        }

        private void OnCellSelectSuccess(IGameEvent eventData)
        {
            if (!_bStart || _bPause || !_bModePlayOwner)
                return;
            
            _gameScreen.KeyboardPanel.ShowAsync().Forget();
        }
        
        private void OnLetterPutSuccess(IGameEvent eventData)
        {
            _bLetterPut = true;
            _gameScreen.GoButton.SetActive(true);
            _gameScreen.CancelButton.SetActive(true);
            _audioService?.PlaySfxAsync(AssetKey.sfx_letter_put_success.ToString());
        }

        private void OnLetterPutToWord(LetterPutToWordEvent eventData)
        {
            _gameScreen.AddLetterToWord(eventData.letter);
            _audioService?.PlaySfxAsync(AssetKey.sfx_letter_selected.ToString());
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
            
            var popup = await _ui.ShowPopupAsync<AdvicePopup, MessageBoxData>(AssetKey.AdvicePopup, messageBoxData);
            
            _audioService?.PlaySfxAsync(AssetKey.sfx_popup_worning.ToString());
            
            await popup.WaitForResultAsync();
            
            switch(eventData.GameError)
            {
                case GameError.SET_LETTER_NO_SELECTED:
                    BlockUIAsync(true, BlockUIScreenMode.NoSpinner);
                    _gameScreen.CancelButton.SetActive(false);
                    _gameScreen.GoButton.SetActive(false);
                    _wordsFieldManager.BlinkNoSelectedLetter();
                    break;
                case GameError.WORD_ALREADY_BEEN:
                    Cancel();
                    break;
            }
        }

        private void OnModeBlinkEnd(ModeBlinkEndEvent eventData)
        {
            Cancel();
            BlockUIAsync(false);
        }
        
        private void SaveWordAndContinueGame(string word)
        {
            _gameScreen.BoosterPanel.SlowdownStop();
            _audioService?.PlaySfxAsync(AssetKey.sfx_i_made_move.ToString());
            
            _gameScreen.TimerBar.ResetTimer();
            if (_bModePlayOwner)
            {
                _gameScreen.StatisticsPanel.StatisticPlayerPlayerPanelOwner.AddWord(word);                
                _gameScreen.PlayerPanelOwner.SetScore(_gameScreen.PlayerPanelOwner.Score + (uint)word.Length);
            }
            else
            {
                _gameScreen.StatisticsPanel.StatisticPlayerPlayerPanelOpponent.AddWord(word);
                _gameScreen.PlayerPanelOpponent.SetScore(_gameScreen.PlayerPanelOpponent.Score + (uint)word.Length);
            }
            
            _gameScreen.SetTextWord("");
            
            _wordsFieldManager.SaveWord(word);
            _wordsFieldManager.Clear();
            
            _bLetterPut = false;

            CheckFinishGame();
        }

        private void OnGameSkip(IGameEvent eventData)
        {
            if (!_bStart || _bPause || !_bModePlayOwner)
                return;

            TryConfirmPass().Forget();
        }

        private async UniTask TryConfirmPass()
        {
            if (PlayerPrefs.GetInt(PlayerPrefsKey.ConfirmPassDontShowAgainKey, 0) == 0)
            {
                var popup = await _ui.ShowPopupAsync<ConfirmPassPopup, MessageBoxData>(AssetKey.ConfirmPassPopup, null);
            
                _audioService?.PlaySfxAsync(AssetKey.sfx_popup_question.ToString());
            
                var exitData = await popup.WaitForResultAsync();
            
                if(exitData.Result == PopupResult.Exit)
                    return;
            }
            
            _gameScreen.BoosterPanel.SlowdownStop();
            PassedGame();
        }
        
        private void OnRepeatGame(RepeatGameEvent eventData)
        {
            _gameScreen.RepeatGame.SetActive(false);
            RepeatGame();
        }
        
        private async UniTask RepeatGame()
        {
            // Пытаемся показать interstitial и ждём закрытия (если показалась)
            await _interstitialService.TryShowAndWaitAsync("exit_game");

            _gameScreen.Reset();
            _saveGameData = _saveGameDataRepeat;
            EventBus.Raise(new GameScreenStartEvent{ Screen = _gameScreen, Opponent = _gameOpponent });
        }
        
        private void OnTimeExpired(IGameEvent eventData)
        {
            _ai.AbortSearch(); // Для подстраховки (при использовании бустера таймер останавливается)
            
            _gameScreen.StatisticsPanel.HideAsync().Forget();
            _gameScreen.KeyboardPanel.HideAsync().Forget();
            
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
            
            _audioService?.PlaySfxAsync(AssetKey.sfx_pass.ToString());
            
            CheckFinishGame();
        }

        private void OnOpponentFindWordSuccess(OpponentFindWordEvent eventData)
        {
            _gameScreen.TimerBar.StopTimer();
            _gameScreen.SetTextWord(eventData.word);
            _gameScreen.StartCoroutine(DisplayWordOpponent(eventData));
            
            _audioService?.PlaySfxAsync(AssetKey.sfx_opponent_made_move.ToString());
        }

        private IEnumerator DisplayWordOpponent(OpponentFindWordEvent eventData)
        {
            yield return new WaitForSeconds(2);
            SaveWordAndContinueGame(eventData.word);
        }

        private void OnOpponentFindWordFail(IGameEvent eventData)
        {
            _gameScreen.PlayerPanelOpponent.SetPass(_gameScreen.PlayerPanelOpponent.Pass + 1, _maxPasses);
            _audioService?.PlaySfxAsync(AssetKey.sfx_opponent_find_word_fail.ToString());
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
                BlockUIAsync(false);
                _gameScreen.SetStatusLocalizationKey("STATUS_LABEL_GO_OWNER");
                _gameScreen.PauseButton.interactable = true;
                _gameScreen.PassButton.interactable = true;
            }
            else
            {
                _gameScreen.SetStatusLocalizationKey("STATUS_LABEL_GO_OPPONENT");
                _wordsFieldManager.SetModeSelect(false);
                _gameScreen.PauseButton.interactable = false;
                _gameScreen.PassButton.interactable = false;
                _gameScreen.CancelButton.SetActive(false);
                _gameScreen.GoButton.SetActive(false);
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
            _gameScreen.PauseButton.interactable = false;
            _gameScreen.PassButton.interactable = false;
            _gameScreen.CancelButton.SetActive(false);
            _gameScreen.GoButton.SetActive(false);
            
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
        
        protected async UniTaskVoid ShowFinishGamePopup(ResultGame resultGame)
        {
            _profile.AddScoreAsync((int)_gameScreen.PlayerPanelOwner.Score);

            await _ui.HideAllPopupsAsync();

            var data = new FinishGamePopupData(
                _gameScreen.PlayerPanelOwner.PlayerName,
                _gameScreen.PlayerPanelOpponent.PlayerName,
                _gameScreen.PlayerPanelOwner.Score,
                _gameScreen.PlayerPanelOpponent.Score,
                _gameScreen.PlayerPanelOwner.Pass,
                _gameScreen.PlayerPanelOpponent.Pass,
                _maxPasses
            );

            FinishGamePopup finishPopup;
            switch (resultGame)
            {
                case ResultGame.OWNER_WIN:
                    finishPopup = await _ui.ShowPopupAsync<FinishGamePopup, FinishGamePopupData>(AssetKey.WinPopup, data);
                    _audioService?.PlaySfxAsync(AssetKey.sfx_i_won.ToString());
                    break;

                case ResultGame.OWNER_LOSE:
                    finishPopup = await _ui.ShowPopupAsync<FinishGamePopup, FinishGamePopupData>(AssetKey.LosePopup, data);
                    _audioService?.PlaySfxAsync(AssetKey.sfx_opponent_won.ToString());
                    break;

                default:
                    finishPopup = await _ui.ShowPopupAsync<FinishGamePopup, FinishGamePopupData>(AssetKey.DrawPopup, data);
                    _audioService?.PlaySfxAsync(AssetKey.sfx_draw.ToString());
                    break;
            }

            await finishPopup.WaitForResultAsync();

            _gameScreen.RepeatGame.SetActive(true);
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
            _gameScreen.TimerBar.StopTimer();
            
            Cancel(); // "очистить мусор"

            var boosterSettings = _configService.Game.GetComplexityAIItem(ComplexityAI.HARD);
            var res = await _ai.FindWordAsync(boosterSettings);

            if (res.Success)
            {
                ShowBoosterLetterSuccess(res.Word);
                
                await UniTask.WaitForSeconds(_configService.Game.autoApplyDelay);
                
                SaveWordAndContinueGame(res.Word);
            }
            else
            {
                await ShowBoosterLetterFailAsync();
                _gameScreen.TimerBar.StartTimer();
            }
        }

        private void ShowBoosterLetterSuccess(string resWord)
        {
            _gameScreen.SetTextWord(resWord);
            _wordsFieldManager.SetModeSelect(true);
            _bLetterPut = true;
            _gameScreen.SetStatusLocalizationKey("STATUS_LABEL_BOOSTER_SUCCESS");
            
            BlockUIAsync(true, BlockUIScreenMode.NoSpinner);
            
            _audioService?.PlaySfxAsync(AssetKey.sfx_letter_put_success.ToString());
        }

        private async UniTask ShowBoosterLetterFailAsync()
        {
            _gameScreen.SetStatusLocalizationKey("STATUS_LABEL_BOOSTER_FAIL");

            // серверный возврат
            await _inventorySync.GrantBoosterAsync(BoosterType.Letter, 1);

            // и обновить панель
            _gameScreen.BoosterPanel.Refresh();
        }

        private async UniTask BlockUIAsync(bool isBlocked, BlockUIScreenMode mode = BlockUIScreenMode.Default)
        {
            await _blockUiLock.WaitAsync();
            try
            {
                if (isBlocked)
                    await _ui.ShowBlockUIScreenAsync(AssetKey.BlockUIScreen, mode);
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