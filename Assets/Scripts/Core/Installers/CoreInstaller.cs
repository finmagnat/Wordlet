using Core.Config;
using Core.Services;
using Cysharp.Threading.Tasks;
using UI.Screens;
using UnityEngine;
using Zenject;

namespace Core.Installers
{
    // регистрирует все сервисы.
    public class CoreInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // 🔧 1. Бинды всех сервисов
            Container.Bind<EventBus>().AsSingle();
            Container.Bind<ConfigService>().AsSingle();
            Container.Bind<GameLogger>().AsSingle();

            // UI и адреса уже приходят из других инсталлеров
        }

        public override void Start()
        {
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            Debug.Log("⚙️ Initializing core services...");

            // 🔧 2. Получаем все сервисы из контейнера
            var eventBus = Container.Resolve<EventBus>();
            var configService = Container.Resolve<ConfigService>();
            var logger = Container.Resolve<GameLogger>();
            var ui = Container.Resolve<UIService>();
            var addresses = Container.Resolve<UIAddresses>();

            // 🔧 3. Последовательная инициализация
            await configService.InitializeAsync();
            await eventBus.InitializeAsync();
            await logger.InitializeAsync();

            // 🔧 4. Запускаем UI
            await ui.ShowScreenAsync<UIScreen>(addresses.MainMenu);

            Debug.Log("✅ All services initialized!");
        }
    }
}