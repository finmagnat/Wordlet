using Core.Config;
using Core.Data;
using Core.Events;
using Core.Generated;
using Cysharp.Threading.Tasks;
using UI.Popups;
using UnityEngine;

namespace UI.Screens
{
    public class AIGameScreen : GameScreenNoPayload
    {
        protected override UniTask PrepareScreenAsync()
        {
            return UniTask.CompletedTask;
        }

        public override async UniTask ShowAsync()
        {
            await base.ShowAsync();

            EventBus.Raise(new GameScreenStartEvent
            {
                Screen = this,
                Opponent = GameOpponent.AI
            });
        }

        protected override async void OnGoToHome(GoToHomeEvent eventData)
        {
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
    }
}