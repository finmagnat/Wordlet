using Core.Config;
using Core.Data;
using Core.Events;
using Core.Generated;
using Cysharp.Threading.Tasks;
using UI.Popups;
using UnityEngine;

namespace UI.Screens
{
    // Экран игры с ИИ.
    public class AIGameScreen : GameScreen
    {
        public override UniTask ShowAsync()
        {
            base.ShowAsync();
            
            EventBus.Raise(new GameScreenStartEvent{ Screen = this, Opponent = GameOpponent.AI});
            
            return UniTask.CompletedTask;
        }
        
        protected override async void OnGoToHome(GoToHomeEvent eventData)
        {
            if (_isProcessing) // Игра не завершена
            {
                // Попап с предложением "Сохранить и выйти" или "Выйти без сохранения".
                var popup = await _ui.ShowPopupAsync<AIGameExitPopup>(AssetKey.AIGameExitPopup);
                var data = await popup.WaitForResultAsync();

                if (data.Result == PopupResult.Exit || data.Result == PopupResult.SaveAndExit)
                    await GoToHome(data.Result == PopupResult.SaveAndExit);
                else
                    Debug.Log("Игрок вернулся в игру");
            }
            else
                await GoToHome();
        }


    }
}