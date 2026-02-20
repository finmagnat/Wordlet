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
        [InjectOptional] private RewardedAdsService _ads;

        [SerializeField] private ShopItemView _itemPrefab;
        [SerializeField] private Transform _contentRoot;
        [SerializeField] private GameObject _header;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private Button _buyButton;
        [SerializeField] private TextMeshProUGUI _ctaText;

        private ShopOfferDto _dto;
        private Action<ShopOfferDto> _onClick;

        private string _idleCtaText;
        private bool _subscribed;

        public void Bind(ShopOfferDto dto, Action<ShopOfferDto> onClick)
        {
            Unsubscribe();

            _dto = dto;
            _onClick = onClick;

            if (!string.IsNullOrEmpty(dto.Title))
            {
                _header.gameObject.SetActive(true);
                _title.text = _localization.Get(LocalizationConst.TableUI, dto.Title);
            }
            else
            {
                _header.gameObject.SetActive(false);
            }

            _idleCtaText = string.IsNullOrWhiteSpace(dto.CtaText) ? "—" : dto.CtaText;
            _ctaText.text = _idleCtaText;

            // очистка наград
            for (int i = _contentRoot.childCount - 1; i >= 0; i--)
                Destroy(_contentRoot.GetChild(i).gameObject);

            foreach (var item in dto.Rewards)
            {
                var view = Instantiate(_itemPrefab, _contentRoot);
                view.Bind(item);
            }

            _buyButton.onClick.RemoveAllListeners();
            _buyButton.onClick.AddListener(OnButtonClick);

            ApplyInitialState();

            if (dto.Type == ShopOfferTypeDto.RewardedAd && _ads != null)
            {
                _ads.OnAvailabilityChanged += OnAdAvailabilityChanged;
                _ads.OnShowingChanged += OnAdShowingChanged;
                _subscribed = true;

                SyncRewardedState();
            }
        }

        private void ApplyInitialState()
        {
            if (_dto.Type == ShopOfferTypeDto.IapPack)
            {
                _buyButton.interactable = _dto.IsAvailable;
                _ctaText.text = _idleCtaText;
                return;
            }

            _buyButton.interactable = _dto.IsAvailable;
            _ctaText.text = _idleCtaText;
        }

        private void SyncRewardedState()
        {
            if (_ads == null) return;

            bool showing = _ads.IsShowing(_dto.RewardType);
            bool ready = _ads.IsReady(_dto.RewardType);

            if (showing)
            {
                SetLoading(true);
                return;
            }

            if (ready) SetReady();
            else SetLoading(false);
        }

        private void OnButtonClick()
        {
            if (_dto == null) return;

            if (_dto.Type == ShopOfferTypeDto.RewardedAd && _ads != null && _ads.IsShowing(_dto.RewardType))
                return;

            if (_dto.Type == ShopOfferTypeDto.RewardedAd)
                SetLoading(true);
            
            Debug.Log($"[ShopPackItemView] Click offer: type={_dto.Type}, rewardType={_dto.RewardType}, title={_dto.Title}");
            
            _onClick?.Invoke(_dto);
        }

        private void OnAdAvailabilityChanged(RewardType type, bool isReady)
        {
            if (_dto == null) return;
            if (_dto.Type != ShopOfferTypeDto.RewardedAd) return;
            if (_dto.RewardType != type) return;

            if (_ads != null && _ads.IsShowing(type))
                return;

            if (isReady) SetReady();
            else SetLoading(false);
        }

        private void OnAdShowingChanged(RewardType type, bool isShowing)
        {
            if (_dto == null) return;
            if (_dto.Type != ShopOfferTypeDto.RewardedAd) return;
            if (_dto.RewardType != type) return;

            if (isShowing) SetLoading(true);
            else SyncRewardedState();
        }

        private void SetReady()
        {
            _buyButton.interactable = true;
            _ctaText.text = _idleCtaText; // обычно "Смотреть"
        }

        // showDots=true -> "..." (нажатие/показ), false -> "Загрузка..." (ждём preload)
        private void SetLoading(bool showDots)
        {
            _buyButton.interactable = false;
            _ctaText.text = showDots ? "..." : _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyLabelLoading);
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            if (_ads == null) return;

            _ads.OnAvailabilityChanged -= OnAdAvailabilityChanged;
            _ads.OnShowingChanged -= OnAdShowingChanged;
            _subscribed = false;
        }

        private void OnDestroy() => Unsubscribe();
    }
}
