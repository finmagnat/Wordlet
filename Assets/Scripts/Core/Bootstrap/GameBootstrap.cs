using Core.Installers;
using Core.Services;
using UnityEngine;

namespace Core.Bootstrap
{
    // живёт в первой сцене (BootstrapScene) и инициализирует ядро.
    public class GameBootstrap : MonoBehaviour
    {
        private async void Awake()
        {
            DontDestroyOnLoad(gameObject);

            // Регистрация сервисов
            ZenjectInstaller.Register(new EventBus());
            ZenjectInstaller.Register(new ConfigService());
            ZenjectInstaller.Register(new GameLogger());

            await ZenjectInstaller.InitializeAsync();

            Debug.Log("✅ All core services initialized!");
            // Тут можно вызвать загрузку главной сцены
        }

        private void OnDestroy()
        {
            ZenjectInstaller.DisposeAll();
        }
    }
}