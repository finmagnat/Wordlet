using System;
using Core.Data;
using DG.Tweening;
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
        [InjectOptional] private RewardedLimitsService _limits;
        [InjectOptional] private RewardedBoosterGrantService _grant;

        [SerializeField] private ShopItemView _itemPrefab;
        [SerializeField] private Transform _contentRoot;
        [SerializeField] private GameObject _header;
        [SerializeField] private Image _headerImage;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private Button _buyButton;
        [SerializeField] private TextMeshProUGUI _ctaText;

        private ShopOfferDto _dto;
        private Action<ShopOfferDto> _onClick;

        private string _idleCtaText;
        private bool _subscribedAds;
        private bool _subscribedReward;

        private float _nextTickTime;
        private Sequence _popSeq;

        public void Bind(ShopOfferDto dto, Action<ShopOfferDto> onClick)
        {
            Unsubscribe();

            _dto = dto;
            _onClick = onClick;

            if (!string.IsNullOrEmpty(dto.Title))
            {
                _header.gameObject.SetActive(true);
                _headerImage.sprite = dto.SpriteHeader;
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

            // Rewarded wiring
            if (dto.Type == ShopOfferTypeDto.RewardedAd)
            {
                if (_ads != null)
                {
                    _ads.OnAvailabilityChanged += OnAdAvailabilityChanged;
                    _ads.OnShowingChanged += OnAdShowingChanged;
                    _subscribedAds = true;
                }

                if (_grant != null)
                {
                    _grant.OnRewardGranted += OnRewardGranted;
                    _subscribedReward = true;
                }

                if (_limits != null)
                    _limits.OnStateChanged += OnLimitsChanged;

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

        private void Update()
        {
            if (_dto == null) return;
            if (_dto.Type != ShopOfferTypeDto.RewardedAd) return;
            if (_limits == null) return;

            // Обновляем кнопку раз в 1 секунду, если есть cooldown
            if (Time.unscaledTime < _nextTickTime) return;
            _nextTickTime = Time.unscaledTime + 1f;

            SyncRewardedState();
        }

        private void SyncRewardedState()
        {
            // 1) если показывается — "..."
            if (_ads != null && _ads.IsShowing(_dto.RewardType))
            {
                SetLoading(true);
                return;
            }

            // 2) лимиты (daily/cooldown)
            if (_limits != null)
            {
                bool can = _limits.CanClaim(_dto.RewardType, out int remain, out bool dailyReached);

                if (!can)
                {
                    if (dailyReached)
                    {
                        SetBlockedDaily();
                        return;
                    }

                    if (remain > 0)
                    {
                        SetCooldown(remain);
                        return;
                    }
                }
            }

            // 3) готовность рекламы
            if (_ads == null)
            {
                // нет сервиса рекламы — не показываем ошибку, просто выключаем
                SetLoading(false);
                return;
            }

            if (_ads.IsReady(_dto.RewardType))
                SetReady();
            else
                SetLoading(false);
        }

        private void OnButtonClick()
        {
            if (_dto == null) return;

            // Если лимит/кулдаун не позволяет — просто обновим текст и выйдем
            if (_limits != null)
            {
                bool can = _limits.CanClaim(_dto.RewardType, out int remain, out bool dailyReached);
                if (!can)
                {
                    if (dailyReached) SetBlockedDaily();
                    else if (remain > 0) SetCooldown(remain);
                    else SetLoading(false);
                    return;
                }
            }

            if (_dto.Type == ShopOfferTypeDto.RewardedAd && _ads != null && _ads.IsShowing(_dto.RewardType))
                return;

            if (_dto.Type == ShopOfferTypeDto.RewardedAd)
                SetLoading(true);

            _onClick?.Invoke(_dto);
        }

        private void OnAdAvailabilityChanged(RewardType type, bool isReady)
        {
            if (_dto == null) return;
            if (_dto.Type != ShopOfferTypeDto.RewardedAd) return;
            if (_dto.RewardType != type) return;

            // не трогаем пока показывается
            if (_ads != null && _ads.IsShowing(type))
                return;

            SyncRewardedState();
        }

        private void OnAdShowingChanged(RewardType type, bool isShowing)
        {
            if (_dto == null) return;
            if (_dto.Type != ShopOfferTypeDto.RewardedAd) return;
            if (_dto.RewardType != type) return;

            if (isShowing) SetLoading(true);
            else SyncRewardedState();
        }

        private void OnLimitsChanged(RewardType type)
        {
            if (_dto == null) return;
            if (_dto.Type != ShopOfferTypeDto.RewardedAd) return;
            if (_dto.RewardType != type) return;

            SyncRewardedState();
        }

        private void OnRewardGranted(RewardType type, int amount)
        {
            if (_dto == null) return;
            if (_dto.Type != ShopOfferTypeDto.RewardedAd) return;
            if (_dto.RewardType != type) return;

            SyncRewardedState(); // сразу обновим (появится cooldown)
        }

        private void SetReady()
        {
            _buyButton.interactable = true;
            _ctaText.text = _idleCtaText; // обычно "Смотреть"
        }

        private void SetBlockedDaily()
        {
            _buyButton.interactable = false;
            _ctaText.text = _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyTextLimit);
        }

        private void SetCooldown(int remainSeconds)
        {
            _buyButton.interactable = false;
            _ctaText.text = $"{_localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyTextThrough)} {FormatMMSS(remainSeconds)}";
        }

        // showDots=true -> "..." (нажатие/показ), false -> "Загрузка..." (ждём preload)
        private void SetLoading(bool showDots)
        {
            _buyButton.interactable = false;
            _ctaText.text = showDots ? "..." : _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyLabelLoading);
        }

        private static string FormatMMSS(int seconds)
        {
            if (seconds < 0) seconds = 0;
            int m = seconds / 60;
            int s = seconds % 60;
            return $"{m:00}:{s:00}";
        }

        private void Unsubscribe()
        {
            if (_subscribedAds && _ads != null)
            {
                _ads.OnAvailabilityChanged -= OnAdAvailabilityChanged;
                _ads.OnShowingChanged -= OnAdShowingChanged;
                _subscribedAds = false;
            }

            if (_limits != null)
                _limits.OnStateChanged -= OnLimitsChanged;

            if (_subscribedReward && _grant != null)
            {
                _grant.OnRewardGranted -= OnRewardGranted;
                _subscribedReward = false;
            }

            _popSeq?.Kill();
            _popSeq = null;
        }

        private void OnDestroy() => Unsubscribe();
    }
}