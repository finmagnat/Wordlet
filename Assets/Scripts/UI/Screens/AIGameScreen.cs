using Core.Config;
using Core.Data;
using Core.Events;
using Core.Generated;
using Core.Services;
using Cysharp.Threading.Tasks;
using UI.Popups;
using UnityEngine;
using Zenject;

namespace UI.Screens
{
    public class AIGameScreen : GameScreenNoPayload
    {
        [Inject] private AnalyticsService _analytics;
        
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

                if (data.Result == PopupResult.Exit || data.Result == PopupResult.SaveAndExit)
                    await GoToHome(data.Result == PopupResult.SaveAndExit);
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
            _analytics.TrackEvent(AnalyticsEvents.GameFlow.HistoryGameClicked);
        }
    }
}
