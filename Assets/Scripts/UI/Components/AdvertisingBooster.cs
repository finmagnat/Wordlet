using Core.Services;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Random = UnityEngine.Random;

namespace Core.UI.Components
{
    public class AdvertisingBooster : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _labelText;
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private Image _boosterIcon;

        [Inject] private LocalizationService _localization;
        [Inject] private AdvertisingBoosterService _adBoosterService;
        [Inject] private ConfigService _configService;
        [Inject] private ISpriteService _spritesService;

        private AdsRewardItem _data;
        
        public async UniTask ShowAsync()
        {
            await InitialiseAsync();
        }
        
        public void OnClick()
        {
            _adBoosterService.Exequte(_data);
        }
        
        private async UniTask InitialiseAsync()
        {
            _data = _adBoosterService.GetData();
            
            _labelText.text = _localization.Get(LocalizationConst.TableAds, _data.LabelLocaleKeys[Random.Range(0, _data.LabelLocaleKeys.Length)]);
            _countText.text = _data.Count.ToString();
            _boosterIcon.sprite = await _spritesService.GetSpriteAsync(_configService.BoostersIcons.GetAlias(_data.BoosterType));
        }
       
    }
}