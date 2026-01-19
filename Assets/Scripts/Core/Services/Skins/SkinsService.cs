using Core.Config;
using Core.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.Services
{
    public class SkinsService : ISkinsService
    {
        public SkinData SkinCurrent { get; private set; }
        public SkinsConfig Config => _skinsConfig;
        
        [Inject] private ConfigService _configService;

        private SkinsConfig _skinsConfig;
            
        public async UniTask InitializeAsync()
        {
            _skinsConfig = _configService.Skins;
            SkinType skinType = (SkinType)PlayerPrefs.GetInt(PlayerPrefsKey.SkinCurrent, (int)_skinsConfig.SkinByDefault);
            
            SkinCurrent = _skinsConfig.GetSkinByType(skinType);
        }
        
        public void SaveSkinCurrent(SkinType skinType)
        {   
            SkinCurrent = _skinsConfig.GetSkinByType(skinType);
            
            PlayerPrefs.SetInt(PlayerPrefsKey.SkinCurrent, (int)skinType);
            PlayerPrefs.Save();
        }
        
    }
}