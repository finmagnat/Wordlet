using Core.Config;
using Core.Data;
using Core.Events;
using Core.Generated;
using Core.Services;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Core.Services.Inventory;
using UI.Popups;
using UnityEngine;
using Zenject;

namespace UI.Screens
{
    public class AIGameScreen : GameScreenNoPayload
    {
        [Inject] private AnalyticsService _analytics;
        [Inject] private ConfigService _configService;
        [Inject] private GameController _gameController;
        [Inject] private GameAnalyticsPayloadFactory _analyticsPayloadFactory;
        [Inject] private IInventoryService _inventory;
        
        protected override UniTask PrepareScreenAsync()
        {
            return UniTask.CompletedTask;
        }

        public override async UniTask ShowAsync()
        {
            await base.ShowAsync();
            
            _analytics.TrackEvent(AnalyticsEvents.Navigation.AiGameScreenShown);
            
            EventBus.Raise(new GameScreenStartEvent
            {
                Screen = this,
                Opponent = GameOpponent.AI
            });
        }

        protected override async void OnGoToHome(GoToHomeEvent eventData)
        {
            _analytics.TrackEvent(AnalyticsEvents.Navigation.AiGameHomeClicked);
            
            if (_isProcessing)
            {
                var popup = await _ui.ShowPopupAsync<AIGameExitPopup>(AssetKey.AIGameExitPopup);
                var data = await popup.WaitForResultAsync();
                var gameData = _gameController.GetGameData();
                uint maxPasses = _configService.Game.GetComplexityAIItem((ComplexityAI)gameData.levelComplexityAI).MaxPasses;

                if (data.Result == PopupResult.Exit)
                {
                    _analytics.TrackEvent(
                        AnalyticsEvents.Navigation.NoAiGameExitPopupClicked,
                        _analyticsPayloadFactory.CreateGameSnapshotPayload(gameData, maxPasses, _inventory.Boosters));
                    await GoToHome(false);
                }
                else if (data.Result == PopupResult.SaveAndExit)
                {
                    _analytics.TrackEvent(
                        AnalyticsEvents.Navigation.YesAiGameExitPopupClicked,
                        _analyticsPayloadFactory.CreateGameSnapshotPayload(gameData, maxPasses, _inventory.Boosters));
                    await GoToHome(true);
                }
                else
                    Debug.Log("Игрок вернулся в игру");
            }
            else
            {
                await GoToHome();
            }
        }

        protected override void OnPausePressed()
        {
            _analytics.TrackEvent(
                AnalyticsEvents.GameFlow.PauseGameClicked,
                new System.Collections.Generic.Dictionary<string, object>
                {
                    [AnalyticsEvents.Parameter.State] = _isPaused
                        ? AnalyticsEvents.Option.Off
                        : AnalyticsEvents.Option.On
                });
        }

        protected override void OnStatisticOpened()
        {
            SetPause(true);
            _analytics.TrackEvent(AnalyticsEvents.GameFlow.HistoryGameClicked);
        }
    }
}
