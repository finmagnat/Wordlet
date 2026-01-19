using Core.Data;
using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Services.Shop
{
    public class ShopItemView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _countText;
        
        [Header("Shop Item Images")]
        [SerializeField] private Sprite _spriteWordlet;
        [SerializeField] private Sprite _spriteSlowdown;
        
        public void Bind(ShopRewardDto dtoItem)
        {
            switch (dtoItem.ItemId)
            {
                case BoosterType.Letter:
                    _iconImage.sprite = _spriteWordlet;
                    break;
                case BoosterType.Slowdown:
                    _iconImage.sprite = _spriteSlowdown;
                    break;
            }
            _countText.text = dtoItem.Amount.ToString();
        }
    }
}