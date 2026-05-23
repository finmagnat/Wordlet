using Core.Config;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.Services
{
    /* Загружает список с ScriptableObject
     * Использование во внешнем коде (Zenject-DI автоматически подставит нужный тип),
     * например: 
     * [Inject] private GameConfig _gameConfig;
     * [Inject] private UIAddresses _addresses;
     */
    public class ConfigService : IConfigService
    {
        public GameConfig Game => _gameConfig;
        public SkinsConfig Skins => _skinsConfig;
        public ShopCatalog Shop => _shopConfig;
        public AdsConfig Ads => _adsConfig;
        public SoundsConfig Sounds => _soundsConfig;
        public DailyBonusVisualConfig DailyBonusVisual => _dailyBonusVisualConfig;
        
        [Inject(Optional = true)] private GameConfig _gameConfig;     // приходит из инсталлера
        [Inject(Optional = true)] private SkinsConfig _skinsConfig;
        [Inject(Optional = true)] private ShopCatalog _shopConfig;
        [Inject(Optional = true)] private AdsConfig _adsConfig;
        [Inject(Optional = true)] private SoundsConfig _soundsConfig;
        [Inject(Optional = true)] private DailyBonusVisualConfig _dailyBonusVisualConfig;
        // при желании добавляй другие конфиги таким же образом

        public async UniTask InitializeAsync()
        {
            await UniTask.Yield();

            if (_gameConfig == null)
                Debug.LogWarning("⚠️ GameConfig not injected (not in container)?");
            else
                Debug.Log($"Config: referenceRes={_gameConfig.referenceResolution}, match={_gameConfig.screenMatch}");
        }

    }
}
