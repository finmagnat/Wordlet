using Core.Config;
using Core.Services;
using Core.UI;
using Cysharp.Threading.Tasks;
using Tests;
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
            Container.Bind<IUIManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<LocalizationService>().AsSingle().NonLazy();

        }

        public override void Start()
        {
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            var ui = Container.Resolve<IUIManager>();
            var addresses = Container.Resolve<UIAddresses>();

            // 1) Показать экран загрузки
            var loading = await ui.ShowScreenAsync<LoadingScreen>(addresses.LoadingScreen);
            loading.SetProgress01(0.05f);

            // 2) Инициализация сервисов с прогрессом
            await Container.Resolve<ConfigService>().InitializeAsync();
            loading.SetProgress01(0.30f);

            await Container.Resolve<EventBus>().InitializeAsync();
            loading.SetProgress01(0.40f);

            await Container.Resolve<GameLogger>().InitializeAsync();
            loading.SetProgress01(0.50f);
            
            await Container.Resolve<LocalizationService>().InitializeAsync();
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