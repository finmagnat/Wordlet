using System;
using Core.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Core.Services.Shop
{
    public sealed class ShopPackItemView : MonoBehaviour
    {
        [Inject] private LocalizationService _localization;
        
        [SerializeField] private ShopItemView _itemPrefab;
        [SerializeField] private Transform _contentRoot;
        [SerializeField] private GameObject _header;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private Button _buyButton;
        [SerializeField] private TextMeshProUGUI _priceText;

        private string _productId;
        private Action<string> _onBuy;
        
        public void Bind(ShopPackDto dto, Action<string> onBuy)
        {
            _productId = dto.ProductId;
            _onBuy = onBuy;

            if (!string.IsNullOrEmpty(dto.Title))
            {
                _header.gameObject.SetActive(true);
                _title.text = _localization.Get(LocalizationConst.TableUI, dto.Title);
            }
            else
            {
                _header.gameObject.SetActive(false);
            }
            
            _priceText.text = dto.PriceText;
            
            // очистка
            for (int i = _contentRoot.childCount - 1; i >= 0; i--)
                Destroy(_contentRoot.GetChild(i).gameObject);
            
            foreach (var item in dto.Rewards)
            {
                var view = Instantiate(_itemPrefab, _contentRoot);
                view.Bind(item);
            }

            _buyButton.interactable = dto.IsAvailable;
            _buyButton.onClick.RemoveAllListeners();
            _buyButton.onClick.AddListener(() => _onBuy?.Invoke(_productId));
        }
    }
}