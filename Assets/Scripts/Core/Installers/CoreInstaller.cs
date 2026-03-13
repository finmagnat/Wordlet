using System;
using System.Collections.Generic;
using Core.Build;
using Core.DataDictionary;
using Core.Generated;
using Core.Services;
using Core.Services.NewWords;
using Core.Services.Shop;
using Core.UI;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Inventory;
using UI.Popups;
using UI.Screens;
using UI.UI;
using UnityEngine;
using Zenject;

namespace Core.Installers
{
    // регистрирует все сервисы.
    public class CoreInstaller : MonoInstaller
    {
        [Inject] private DictionaryManagerPresenter _dictPresenter;

        public override void InstallBindings() 
        {
            Debug.Log($"<color=yellow>BUILD: {BuildInfo.VersionName} code={BuildInfo.AndroidVersionCode} utc={BuildInfo.Utc}</color>");
            
            // 🔧 1. Бинды всех сервисов
            Container.Bind<GameLogger>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<ConfigService>().AsSingle().NonLazy();
            Container.Bind<LocalizationService>().AsSingle().NonLazy();
            
            Container.Bind<ILoadingUI>().FromComponentInHierarchy().AsSingle();
            
            Container.Bind<AddressablesLoader>().AsSingle();
            Container.Bind<IUIManager>().To<UIManager>().FromComponentInHierarchy().AsSingle();
            
            Container.Bind<IGamePauseService>().To<GamePauseService>().AsSingle();
            Container.Bind<IInternetConnectionService>().To<InternetConnectionService>().AsSingle();

            Container.Bind<ISpriteService>().To<SpriteService>().AsSingle();
            
            Container.Bind<List<LanguageDictionaryConfig>>().FromInstance(_dictPresenter.configs).AsSingle();
            Container.Bind<DictionaryService>().AsSingle();
            Container.Bind<DictionaryManager>().AsSingle().NonLazy();
            
            Container.BindInterfacesAndSelfTo<AudioService>().AsSingle().NonLazy();

            Container.Bind<SkinsService>().AsSingle().NonLazy();
            
            Container.Bind<GameController>().AsSingle();
            
            Container.Bind<ISaveService>().To<SaveService>().AsSingle().NonLazy();
            Container.Bind<IInventoryService>().To<InventoryService>().AsSingle().NonLazy();
            
            Container.BindInterfacesAndSelfTo<PlayFabAuthService>().AsSingle();
            Container.BindInterfacesAndSelfTo<ProfileService>().AsSingle();
            Container.BindInterfacesAndSelfTo<InventorySyncService>().AsSingle();
            
            Container.Bind<INewWordsProvider>().To<PlayFabNewWordsProvider>().AsSingle();
            Container.Bind<INewWordsService>().To<NewWordsService>().AsSingle();
            Container.BindInterfacesAndSelfTo<NewWordsLimitsService>().AsSingle().NonLazy();
            Container.Bind<MissingWordPopupPresenter>().AsSingle();
            
#if UNITY_ANDROID && !UNITY_EDITOR
            Container.Bind<IShopService>().To<GooglePlayShopService>().AsSingle().NonLazy();
#else
            Container.Bind<IShopService>().To<StubShopService>().AsSingle().NonLazy();
#endif
            Container.BindInterfacesAndSelfTo<RewardedAdsService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<RewardedBoosterGrantService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<RewardedLimitsService>().AsSingle().NonLazy();
            
            Container.Bind<AdsEntitlementService>().AsSingle();
            Container.Bind<InterstitialAdsService>().AsSingle();
            Container.Bind<InterstitialPolicyService>().AsSingle();
        }

        public override void Start()
        {
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            var ui = Container.Resolve<IUIManager>();
            var loadingUI = Container.Resolve<ILoadingUI>();
            var internetService = Container.Resolve<IInternetConnectionService>();

            // 1) Показать экран загрузки
            var loading = await loadingUI.ShowLoadingAsync<LoadingScreen>(AssetKey.LoadingScreen);
            loading.SetProgress(0.05f);

            // 2) Инициализация сервисов с прогрессом
            await Container.Resolve<ConfigService>().InitializeAsync();
            loading.SetProgress(0.10f);

            await Container.Resolve<GameLogger>().InitializeAsync();
            loading.SetProgress(0.15f);
            
            await Container.Resolve<LocalizationService>().InitializeAsync();
            loading.SetProgress(0.20f);
            
            await internetService.InitializeAsync();
            if (!await internetService.CheckNowAsync())
            {
                // апку запустили без интернета...
                await loadingUI.HideLoadingAsync();

                // ждём интернет
                while (!await internetService.CheckNowAsync())
                    await UniTask.Delay(500, ignoreTimeScale: true); // ignoreTimeScale на случай паузы

                await loadingUI.ShowLoadingAsync<LoadingScreen>(AssetKey.LoadingScreen);
            }
            
            await Container.Resolve<DictionaryManager>().InitializeAsync();
            loading.SetProgress(0.25f);
            
            await Container.Resolve<SkinsService>().InitializeAsync();
            loading.SetProgress(0.30f);

            await Container.Resolve<GameController>().InitializeAsync();
            loading.SetProgress(0.35f);
            
            await Container.Resolve<AudioService>().InitializeAsync();
            loading.SetProgress(0.40f);
            
            await Container.Resolve<IShopService>().InitializeAsync();
            loading.SetProgress(0.50f);
            
            // PlayFab Auth (создание юзера + подарок)
            await Container.Resolve<PlayFabAuthService>().InitializeAsync();
            loading.SetProgress(0.60f);
            
            // ✅ Ads (init + preload rewarded)
            await Container.Resolve<RewardedAdsService>().InitializeAsync();
            await Container.Resolve<AdsEntitlementService>().InitializeAsync();
            await Container.Resolve<InterstitialAdsService>().InitializeAsync();
            await Container.Resolve<InterstitialPolicyService>().InitializeAsync();
            loading.SetProgress(0.65f);
            
            var go = new GameObject("SRDebugAdsBridge");
            DontDestroyOnLoad(go);
            Container.InstantiateComponent<DebugTools.SRDebugAdsBridge>(go);
            
            var newWordsGo = new GameObject("SRDebugNewWordsBridge");
            DontDestroyOnLoad(newWordsGo);
            Container.InstantiateComponent<DebugTools.SRDebugNewWordsBridge>(newWordsGo);
            
            await Container.Resolve<NewWordsLimitsService>().InitializeAsync();
            
            await Container.Resolve<RewardedBoosterGrantService>().InitializeAsync();
            await Container.Resolve<RewardedLimitsService>().InitializeAsync();
            loading.SetProgress(0.70f);
            
            // ✅ Profile (nickname + score + leaderboard base)
            await Container.Resolve<ProfileService>().InitializeAsync();
            loading.SetProgress(0.75f);

            // Inventory sync (PlayFab -> Local)
            await Container.Resolve<InventorySyncService>().InitializeAsync();
            loading.SetProgress(0.80f);

            // 3) UI → Main Menu
            loading.SetProgress(1.0f);
            await ui.HideAllScreensAsync();
            await ui.ShowScreenAsync<UIScreen>(AssetKey.MainMenuScreen);
            await loadingUI.HideLoadingAsync();
        }

        private void OnDestroy()
        {
            Container.Resolve<GameController>().Dispose();
        }
    }
}