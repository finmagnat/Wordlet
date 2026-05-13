using Core.Data;
using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Services.Shop
{
    public class ShopItemView : MonoBehaviour
    {
        [SerializeField] private GameObject _adsPanel;
        [SerializeField] private GameObject _boosterPanel;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _countText;
        
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
            }
            
            _iconImage.sprite = dtoItem.SpriteIcon;
        }
    }
}