using Core.Config;
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
    public interface IGameBoosterHost
    {
        bool IsGameStarted { get; }
        bool IsPaused { get; }
        bool IsOwnerTurn { get; }
        int RoundDurationSeconds { get; }
        string LocaleCode { get; }

        UniTask BlockUIAsync(bool isBlocked, BlockUIScreenMode mode = BlockUIScreenMode.Default);
        void CancelCurrentMove();
        void SaveWordAndContinueGame(string word);
        void MarkLetterPlacedByBooster();
    }

    public sealed class GameBoosterController
    {
        [Inject] private InventorySyncService _inventorySync;
        [Inject] private ConfigService _configService;
        [Inject] private AudioService _audioService;
        [Inject] private IUIManager _ui;
        [Inject] private AnalyticsService _analytics;
        [Inject] private IInventoryService _inventory;

        private WordsFieldManager _wordsFieldManager;
        private AIGameController _ai;
        private GameScreenBase _gameScreen;
        private IGameBoosterHost _activeEraserHost;

        private bool _bModeEraser;
        private bool _bLetterRemoved;
        private bool _boosterProcessing;

        public bool IsSlowdownActive => _gameScreen != null && _gameScreen.BoosterPanel.IsActive(BoosterType.Slowdown);

        public void Attach(GameScreenBase gameScreen, WordsFieldManager wordsFieldManager, AIGameController ai)
        {
            if (_gameScreen?.EraserOverlay != null)
                _gameScreen.EraserOverlay.CloseButtonClicked -= OnEraserOverlayCloseButtonClicked;

            _gameScreen = gameScreen;
            _wordsFieldManager = wordsFieldManager;
            _ai = ai;

            if (_gameScreen?.EraserOverlay != null)
                _gameScreen.EraserOverlay.CloseButtonClicked += OnEraserOverlayCloseButtonClicked;
        }

        public void ResetForNewGame()
        {
            _bModeEraser = false;
            _bLetterRemoved = false;
            _boosterProcessing = false;
            _activeEraserHost = null;
        }

        public void ResetForOpponentTurn()
        {
            _bLetterRemoved = false;
        }

        public void OnGameFinished()
        {
            CancelEraserMode();
            _bLetterRemoved = false;
            _boosterProcessing = false;
            _activeEraserHost = null;
            _gameScreen?.BoosterPanel.SlowdownStop();
        }

        public void CancelEraserMode()
        {
            if (!_bModeEraser || _gameScreen == null || _wordsFieldManager == null)
                return;

            _bModeEraser = false;
            _activeEraserHost = null;
            _wordsFieldManager.SetModeEraser(false);
            _gameScreen.WordsField.SetModeEraser(false);
            _gameScreen.EraserOverlay.HideAsync().Forget();
        }

        public void OnCellSelectSuccess(CellSelectSuccessEvent eventData)
        {
            if (!_bModeEraser || _gameScreen == null || _wordsFieldManager == null)
                return;

            TrackEraserBoosterSuccess(_activeEraserHost, eventData);
            CancelEraserMode();
            _bLetterRemoved = true;
        }

        public void StopSlowdown()
        {
            _gameScreen?.BoosterPanel.SlowdownStop();
        }

        public async UniTask HandleUseAsync(UseBoosterEvent eventData, IGameBoosterHost host)
        {
            if (eventData.isEmpty)
            {
                _ui.ShowPopupAsync<ShopPopup>(AssetKey.ShopPopup).Forget();
                return;
            }

            if (_gameScreen == null || _wordsFieldManager == null || _ai == null)
                return;

            if (!host.IsGameStarted || host.IsPaused || !host.IsOwnerTurn || _boosterProcessing)
                return;

            if (_gameScreen.BoosterPanel.IsActive(eventData.boosterType))
                return;

            if (_bLetterRemoved && eventData.boosterType == BoosterType.Eraser)
            {
                Debug.Log("[BOOSTER ERASER] ERROR: Можно стереть только одну букву за ход");
                if (!_gameScreen.EraseBubble.IsVisible)
                    _gameScreen.EraseBubble.ShowAsync().Forget();
                return;
            }

            _boosterProcessing = true;
            await host.BlockUIAsync(true);

            bool ok = await _inventorySync.TryUseBoosterAsync(eventData.boosterType);
            _gameScreen.BoosterPanel.Refresh();

            if (!ok)
            {
                await host.BlockUIAsync(false);
                _boosterProcessing = false;
                return;
            }

            switch (eventData.boosterType)
            {
                case BoosterType.Letter:
                    await ActivateBoosterLetterAsync(host);
                    break;

                case BoosterType.Slowdown:
                    ActivateBoosterSlowdownAsync(host).Forget();
                    break;

                case BoosterType.Eraser:
                    await ActivateBoosterEraserAsync(host);
                    break;
            }

            await host.BlockUIAsync(false);
            _boosterProcessing = false;
        }

        private async UniTask ActivateBoosterEraserAsync(IGameBoosterHost host)
        {
            if (!host.IsGameStarted || host.IsPaused || !host.IsOwnerTurn)
                return;

            _audioService?.PlaySfxAsync(SoundsConfig.BoosterSlowdownLaunch);

            host.CancelCurrentMove();

            _bModeEraser = true;
            _activeEraserHost = host;
            _wordsFieldManager.SetModeEraser(true);
            _gameScreen.WordsField.SetModeEraser(true);
            TrackEraserBoosterShown(host);
            await _gameScreen.EraserOverlay.ShowAsync();
        }

        private async UniTask ActivateBoosterLetterAsync(IGameBoosterHost host)
        {
            _gameScreen.TimerBar.StopTimer();
            host.CancelCurrentMove();

            var res = await _ai.FindWordAsync(_configService.Game.boosterLetterAiSettings);
            
            if (res.Success)
            {
                TrackLetterBoosterSuccess(host, res.Word);
                ShowBoosterLetterSuccess(host, res.Word);

                await UniTask.WaitForSeconds(_configService.Game.autoApplyDelay);

                host.SaveWordAndContinueGame(res.Word);
            }
            else
            {
                TrackLetterBoosterFail(host);
                await ShowBoosterLetterFailAsync();
                _gameScreen.TimerBar.StartTimer();
            }
        }

        private void ShowBoosterLetterSuccess(IGameBoosterHost host, string resWord)
        {
            _gameScreen.SetTextWord(resWord);
            _wordsFieldManager.SetModeSelect(true);
            host.MarkLetterPlacedByBooster();
            _gameScreen.SetStatusLocalizationKey("STATUS_LABEL_BOOSTER_SUCCESS");

            host.BlockUIAsync(true, BlockUIScreenMode.NoSpinner).Forget();

            _audioService?.PlaySfxAsync(SoundsConfig.BoosterFoundWord);
        }

        private async UniTask ShowBoosterLetterFailAsync()
        {
            _gameScreen.SetStatusLocalizationKey("STATUS_LABEL_BOOSTER_FAIL");
            _audioService?.PlaySfxAsync(SoundsConfig.BoosterNotFoundWord);

            await _inventorySync.GrantBoosterAsync(BoosterType.Letter, 1);
            _gameScreen.BoosterPanel.Refresh();
        }

        private void TrackLetterBoosterSuccess(IGameBoosterHost host, string word)
        {
            var boardData = _wordsFieldManager.WordsFieldData.GetBoardData();
            int emptyCells = 0;

            for (int i = 0; i < boardData.Length; i++)
            {
                if (string.IsNullOrEmpty(boardData[i]))
                    emptyCells++;
            }

            _analytics.TrackEvent(AnalyticsEvents.BoosterUsage.LetterBoosterSuccess, new System.Collections.Generic.Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Word] = word,
                [AnalyticsEvents.Parameter.WordLength] = word.Length,
                [AnalyticsEvents.Parameter.Locale] = host.LocaleCode,
                [AnalyticsEvents.Parameter.CellsEmpty] = emptyCells,
                [AnalyticsEvents.Parameter.Field] = AnalyticsPayloadHelper.GetFieldPayload(boardData)
            });
        }

        private void TrackLetterBoosterFail(IGameBoosterHost host)
        {
            var boardData = _wordsFieldManager.WordsFieldData.GetBoardData();
            int emptyCells = 0;

            for (int i = 0; i < boardData.Length; i++)
            {
                if (string.IsNullOrEmpty(boardData[i]))
                    emptyCells++;
            }

            _analytics.TrackEvent(AnalyticsEvents.BoosterUsage.LetterBoosterFail, new System.Collections.Generic.Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Locale] = host.LocaleCode,
                [AnalyticsEvents.Parameter.DurationRound] = host.RoundDurationSeconds,
                [AnalyticsEvents.Parameter.DurationRoundLeft] = Mathf.Max(0, host.RoundDurationSeconds - Mathf.RoundToInt(_gameScreen.TimerBar.GetCurrentValue())),
                [AnalyticsEvents.Parameter.CellsEmpty] = emptyCells,
                [AnalyticsEvents.Parameter.Field] = AnalyticsPayloadHelper.GetFieldPayload(boardData)
            });
        }

        private async UniTaskVoid ActivateBoosterSlowdownAsync(IGameBoosterHost host)
        {
            if (!host.IsGameStarted || host.IsPaused || !host.IsOwnerTurn)
                return;

            _gameScreen.TimerBar.StopTimer();
            _gameScreen.BoosterPanel.SlowdownStart();
            _audioService?.PlaySfxAsync(SoundsConfig.BoosterSlowdownLaunch);

            await UniTask.WaitForSeconds(_configService.Game.slowdownDelay);

            if (!host.IsPaused && host.IsGameStarted)
                _gameScreen.TimerBar.StartTimer();
        }

        private void OnEraserOverlayCloseButtonClicked()
        {
            if (!_bModeEraser)
                return;

            var host = _activeEraserHost;
            CancelEraserMode();
            ReturnEraserBoosterAsync(host).Forget();
        }

        private async UniTaskVoid ReturnEraserBoosterAsync(IGameBoosterHost host)
        {
            await _inventorySync.GrantBoosterAsync(BoosterType.Eraser, 1);
            _gameScreen?.BoosterPanel.Refresh();
            TrackEraserBoosterClosed(host);
        }

        private void TrackEraserBoosterShown(IGameBoosterHost host)
        {
            var boardData = _wordsFieldManager.WordsFieldData.GetBoardData();

            _analytics.TrackEvent(AnalyticsEvents.BoosterUsage.EraserBoosterShown, new System.Collections.Generic.Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Locale] = host.LocaleCode,
                [AnalyticsEvents.Parameter.Field] = AnalyticsPayloadHelper.GetFieldPayload(boardData)
            });
        }

        private void TrackEraserBoosterClosed(IGameBoosterHost host)
        {
            if (host == null)
                return;

            _analytics.TrackEvent(AnalyticsEvents.BoosterUsage.EraserBoosterClosed, new System.Collections.Generic.Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.DurationRound] = host.RoundDurationSeconds,
                [AnalyticsEvents.Parameter.DurationRoundLeft] = Mathf.Max(0, host.RoundDurationSeconds - Mathf.RoundToInt(_gameScreen.TimerBar.GetCurrentValue())),
                [AnalyticsEvents.Parameter.Boosters] = AnalyticsPayloadHelper.GetBoostersPayload(_inventory.Boosters)
            });
        }

        private void TrackEraserBoosterSuccess(IGameBoosterHost host, CellSelectSuccessEvent eventData)
        {
            if (host == null || eventData == null || !eventData.isEraserSuccess || eventData.letter == null)
                return;

            var boardData = _wordsFieldManager.WordsFieldData.GetBoardData();

            _analytics.TrackEvent(AnalyticsEvents.BoosterUsage.EraserBoosterSuccess, new System.Collections.Generic.Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Locale] = host.LocaleCode,
                [AnalyticsEvents.Parameter.Field] = AnalyticsPayloadHelper.GetFieldPayload(boardData),
                [AnalyticsEvents.Parameter.EraseItem] = AnalyticsPayloadHelper.GetIndexedItemPayload(eventData.letter.Index, eventData.erasedLetter),
                [AnalyticsEvents.Parameter.DurationRound] = host.RoundDurationSeconds,
                [AnalyticsEvents.Parameter.DurationRoundLeft] = Mathf.Max(0, host.RoundDurationSeconds - Mathf.RoundToInt(_gameScreen.TimerBar.GetCurrentValue())),
                [AnalyticsEvents.Parameter.Boosters] = AnalyticsPayloadHelper.GetBoostersPayload(_inventory.Boosters)
            });
        }
    }
}
