using Core.Config;
using Core.Services;
using UI.Screens;
using UnityEngine;
using IInitializable = Core.Services.IInitializable;

namespace UI
{
    public class GameBootstrap: IInitializable
    {
        private readonly UIService _ui;
        private readonly UIAddresses _addresses;

        public GameBootstrap(UIService ui, UIAddresses addresses)
        {
            _ui = ui;
            _addresses = addresses;
        }

        public async void Initialize()
        {
            Debug.Log("GameBootstrap.Initialize called");
            await _ui.ShowScreenAsync<UIScreen>(_addresses.MainMenu);
        }

    }
}