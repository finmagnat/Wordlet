using System;
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
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _labelText;
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private Image _boosterIcon;

        [Inject] private LocalizationService _localization;
        [Inject] private AdvertisingBoosterService _adBoosterService;
        [Inject] private ConfigService _configService;
        [Inject] private ISpriteService _spritesService;

        private AdsRewardItem _data;
        private Action _callbackOnClick;
        
        public async UniTask ShowAsync(AdsRewardItem data, Action callbackOnClick)
        {
            _data = data;
            _callbackOnClick = callbackOnClick;
            
            await InitialiseAsync();
        }
        
        public void OnClick()
        {
            if (!_adBoosterService.Execute(_data))
                return;

            _callbackOnClick?.Invoke();
            _canvasGroup.alpha = 0;
        }
        
        private async UniTask InitialiseAsync()
        {
            _labelText.text = _localization.Get(LocalizationConst.TableAds, _data.LabelLocaleKeys[Random.Range(0, _data.LabelLocaleKeys.Length)]);
            
            _countText.text = $"X {_data.Count}";
            _boosterIcon.sprite = await _spritesService.GetSpriteAsync(_configService.BoostersIcons.GetAlias(_data.BoosterType));
            
            _canvasGroup.alpha = 1;
        }
       
    }
}
