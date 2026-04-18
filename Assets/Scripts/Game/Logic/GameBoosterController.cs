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

        private WordsFieldManager _wordsFieldManager;
        private AIGameController _ai;
        private GameScreenBase _gameScreen;

        private bool _bModeEraser;
        private bool _bLetterRemoved;
        private bool _boosterProcessing;

        public bool IsSlowdownActive => _gameScreen != null && _gameScreen.BoosterPanel.IsActive(BoosterType.Slowdown);

        public void Attach(GameScreenBase gameScreen, WordsFieldManager wordsFieldManager, AIGameController ai)
        {
            _gameScreen = gameScreen;
            _wordsFieldManager = wordsFieldManager;
            _ai = ai;
        }

        public void ResetForNewGame()
        {
            _bModeEraser = false;
            _bLetterRemoved = false;
            _boosterProcessing = false;
        }

        public void ResetForOpponentTurn()
        {
            _bLetterRemoved = false;
        }

        public void OnGameFinished()
        {
            _bLetterRemoved = false;
            _boosterProcessing = false;
            _gameScreen?.BoosterPanel.SlowdownStop();
        }

        public void OnCellSelectSuccess()
        {
            if (!_bModeEraser || _gameScreen == null || _wordsFieldManager == null)
                return;

            _bModeEraser = false;
            _bLetterRemoved = true;
            _wordsFieldManager.SetModeEraser(false);
            _gameScreen.WordsField.SetModeEraser(false);
            _gameScreen.EraserOverlay.HideAsync().Forget();
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
            _wordsFieldManager.SetModeEraser(true);
            _gameScreen.WordsField.SetModeEraser(true);
            await _gameScreen.EraserOverlay.ShowAsync();
        }

        private async UniTask ActivateBoosterLetterAsync(IGameBoosterHost host)
        {
            _gameScreen.TimerBar.StopTimer();
            host.CancelCurrentMove();

            var res = await _ai.FindWordAsync(_configService.Game.boosterLetterAiSettings);

            if (res.Success)
            {
                ShowBoosterLetterSuccess(host, res.Word);

                await UniTask.WaitForSeconds(_configService.Game.autoApplyDelay);

                host.SaveWordAndContinueGame(res.Word);
            }
            else
            {
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
    }
}
