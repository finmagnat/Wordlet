using Core.Config;
using Core.Data;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Core.Services.Shop
{
    public class ShopItemView : MonoBehaviour
    {
        [SerializeField] private GameObject _adsPanel;
        [SerializeField] private GameObject _boosterPanel;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _countText;
        
        [Inject] private ConfigService _configService;
        [Inject] private ISpriteService _spritesService;
        
        public void Bind(RewardDto dtoItem)
        {
            if (dtoItem.ItemId == BoosterType.None)
            {
                _boosterPanel.SetActive(false);
                _adsPanel.SetActive(true);
            }
            else
            {
                _boosterPanel.SetActive(true);
                _adsPanel.SetActive(false);
                _countText.text = dtoItem.Amount.ToString();
                SetBoosterIconAsync(dtoItem.ItemId);
            }
        }

        private async UniTask SetBoosterIconAsync(BoosterType itemId)
        {
            _iconImage.sprite = await _spritesService.GetSpriteAsync(_configService.BoostersIcons.GetAlias(itemId));
        }
    }
}