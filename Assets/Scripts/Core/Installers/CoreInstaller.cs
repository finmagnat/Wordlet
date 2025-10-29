using Core.Config;
using Core.Services;
using Cysharp.Threading.Tasks;
using UI.Screens;
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
            var ui = Container.Resolve<UIService>();
            var addresses = Container.Resolve<UIAddresses>();
            var configService = Container.Resolve<ConfigService>();
            Container.Bind<GameConfig>().FromInstance(configService.Get()).AsSingle();
            var eventBus = Container.Resolve<EventBus>();
            var logger = Container.Resolve<GameLogger>();

            // 1) Показать экран загрузки
            var loading = await ui.ShowScreenAsync<LoadingScreen>(addresses.LoadingScreen);
            loading.SetProgress01(0.05f);

            // 2) Инициализация сервисов с прогрессом
            await configService.InitializeAsync();
            loading.SetProgress01(0.30f);

            await eventBus.InitializeAsync();
            loading.SetProgress01(0.45f);

            await logger.InitializeAsync();
            loading.SetProgress01(0.60f);

            // Здесь же можно Addressables.InitializeAsync(), авторизацию, подготовку кэшей и т.п.
            // var initHandle = Addressables.InitializeAsync();
            // await initHandle;
            // loading.SetProgress01(0.75f);

            // 3) UI → Main Menu
            loading.SetProgress01(1.0f);
            await ui.HideAllScreensAsync();
            await ui.ShowScreenAsync<UIScreen>(addresses.MainMenu);
        }
    }
}