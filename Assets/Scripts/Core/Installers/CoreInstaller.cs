using System.Collections.Generic;
using Core.Dictionary;
using Core.Generated;
using Core.Services;
using Core.Services.Shop;
using Core.UI;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Inventory;
using UI.Screens;
using UI.UI;
using Zenject;

namespace Core.Installers
{
    // регистрирует все сервисы.
    public class CoreInstaller : MonoInstaller
    {
        [Inject] private DictionaryManagerPresenter _dictPresenter;

        public override void InstallBindings()
        {
            // 🔧 1. Бинды всех сервисов
            Container.Bind<GameLogger>().AsSingle();
            
            Container.Bind<ConfigService>().AsSingle();
            Container.Bind<LocalizationService>().AsSingle().NonLazy();
            
            Container.Bind<ILoadingUI>().FromComponentInHierarchy().AsSingle();
            
            Container.Bind<AddressablesLoader>().AsSingle();
            Container.Bind<IUIManager>().To<UIManager>().FromComponentInHierarchy().AsSingle();

            Container.Bind<ISpriteService>().To<SpriteService>().AsSingle();
            
            Container.Bind<List<LanguageDictionaryConfig>>().FromInstance(_dictPresenter.configs).AsSingle();
            Container.Bind<DictionaryService>().AsSingle();
            Container.Bind<DictionaryManager>().AsSingle().NonLazy();
            
            Container.BindInterfacesAndSelfTo<AudioService>().AsSingle().NonLazy();

            Container.Bind<SkinsService>().AsSingle().NonLazy();
            
            Container.Bind<GameController>().AsSingle();
            
            Container.Bind<ISaveService>().To<SaveService>().AsSingle().NonLazy();
            Container.Bind<IInventoryService>().To<InventoryService>().AsSingle().NonLazy();
            
            Container.Bind<IShopService>().To<StubShopService>().AsSingle().NonLazy();
        }

        public override void Start()
        {
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            var ui = Container.Resolve<IUIManager>();
            var loadingUI = Container.Resolve<ILoadingUI>();

            // 1) Показать экран загрузки
            var loading = await loadingUI.ShowLoadingAsync<LoadingScreen>(AssetKey.LoadingScreen);
            loading.SetProgress(0.05f);

            // 2) Инициализация сервисов с прогрессом
            await Container.Resolve<ConfigService>().InitializeAsync();
            loading.SetProgress(0.30f);

            await Container.Resolve<GameLogger>().InitializeAsync();
            loading.SetProgress(0.40f);
            
            await Container.Resolve<LocalizationService>().InitializeAsync();
            loading.SetProgress(0.60f);
            
            await Container.Resolve<DictionaryManager>().InitializeAsync();
            loading.SetProgress(0.70f);
            
            await Container.Resolve<SkinsService>().InitializeAsync();
            loading.SetProgress(0.80f);

            await Container.Resolve<GameController>().InitializeAsync();
            loading.SetProgress(0.85f);
            
            await Container.Resolve<AudioService>().InitializeAsync();
            loading.SetProgress(0.90f);
            
            await Container.Resolve<IShopService>().InitializeAsync();
            loading.SetProgress(0.90f);
            
            // Здесь же можно Addressables.InitializeAsync(), авторизацию, подготовку кэшей и т.п.
            // var initHandle = Addressables.InitializeAsync();
            // await initHandle;
            // loading.SetProgress01(0.75f);

            // 3) UI → Main Menu
            loading.SetProgress(1.0f);
            await ui.HideAllScreensAsync();
            await ui.ShowScreenAsync<UIScreen>(AssetKey.MainMenuScreen);
            await loadingUI.HideLoadingAsync();
        }
    }
}