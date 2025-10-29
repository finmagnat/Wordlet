using Core.Config;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Services
{
    // (читает ScriptableObject с настройками игры)
    public class ConfigService : IConfigService
    {
        private GameConfig _config;

        public async UniTask InitializeAsync()
        {
            Debug.Log("Loading Config...");
            await UniTask.Delay(300); // имитация загрузки
            _config = Resources.Load<GameConfig>("Config/GameConfig");
            Debug.Log("Config loaded!");
        }

        public GameConfig Get() => _config;
    }
}