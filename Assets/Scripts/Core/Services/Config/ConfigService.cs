using Core.Config;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.Services
{
    // (читает ScriptableObject с настройками игры)
    public class ConfigService : IConfigService
    {
        private GameConfig _config;
        private readonly DiContainer _container;

        [Inject]
        public ConfigService(DiContainer container)
        {
            _container = container;
        }

        public async UniTask InitializeAsync()
        {
            Debug.Log("📄 ConfigService: loading GameConfig...");
            await UniTask.Yield();

            _config = Resources.Load<GameConfig>("Config/GameConfig");

            if (_config == null)
            {
                Debug.LogError("❌ GameConfig not found in Resources/Config/");
                return;
            }

            // Регистрируем GameConfig в Zenject-контейнере
            _container.Bind<GameConfig>().FromInstance(_config).AsSingle();

            Debug.Log($"✅ ConfigService initialized: {_config.referenceResolution.x}x{_config.referenceResolution.y}, match={_config.screenMatch}");
        }

        public GameConfig Get() => _config;
    }
}