using System;
using Core.Config;
using Core.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.Services
{
    public class SkinsService : ISkinsService
    {
        public event Action<SkinData> OnSkinChanged;
        public SkinData SkinCurrent { get; private set; }
        public bool SkinRandomSelect { get; private set; }
        
        public SkinsConfig Config => _skinsConfig;
        
        [Inject] private ConfigService _configService;

        private SkinsConfig _skinsConfig;
            
        public async UniTask InitializeAsync()
        {
            _skinsConfig = _configService.Skins;
            
            SkinRandomSelect = PlayerPrefs.GetInt(PlayerPrefsKey.SkinSelectRandomKey, 1) == 1;
            
            SkinType skinType = (SkinType)PlayerPrefs.GetInt(PlayerPrefsKey.SkinCurrent, (int)_skinsConfig.SkinByDefault);
            
            SkinCurrent = _skinsConfig.GetSkinByType(skinType);
        }
        
        public void SaveSkinCurrent(SkinType skinType)
        {   
            SkinCurrent = _skinsConfig.GetSkinByType(skinType);
            
            PlayerPrefs.SetInt(PlayerPrefsKey.SkinCurrent, (int)skinType);
            PlayerPrefs.Save();
            
            OnSkinChanged?.Invoke(SkinCurrent);
        }
        
        public void TrySaveRandomSelect(bool value)
        {
            SkinRandomSelect = value;
            PlayerPrefs.SetInt(PlayerPrefsKey.SkinSelectRandomKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetSkinRandom()
        {
            SaveSkinCurrent(_skinsConfig.GetSkinRandom().SkinType);
            Debug.Log($"[SkinsService][SetSkinRandom] SkinCurrent = {SkinCurrent.SkinType}");
        }
    }
}