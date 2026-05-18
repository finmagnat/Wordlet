using System;
using System.Collections.Generic;
using Core.Config;
using Core.Data;
using Core.UI.Components;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Core.Services.Shop
{
    public sealed class ShopPackItemView : MonoBehaviour
    {
        public ShopOfferDto Dto => _dto;
        public int Cooldown => _limitRemain;
        public bool IsLimitDailyReached => _limitDailyReached;

        [Inject] private LocalizationService _localization;
        [Inject] private AnalyticsService _analytics;
        [InjectOptional] private RewardedAdsService _ads;
        [InjectOptional] private RewardedLimitsService _limits;
        [InjectOptional] private RewardedBoosterGrantService _grant;

        [SerializeField] private ShopItemView _itemPrefab;
        [SerializeField] private Transform _contentRoot;
        [SerializeField] private GameObject _header;
        [SerializeField] private Image _headerImage;
        [SerializeField] private LocalizedTMPStringEvent _title;
        [SerializeField] private Button _buyButton;
        [SerializeField] private TextMeshProUGUI _ctaText;

        private ShopOfferDto _dto;
        private Action<ShopOfferDto> _onClick;

        private string _idleCtaText;
        private bool _subscribedAds;
        private bool _subscribedReward;

        private float _nextTickTime;
        private Sequence _popSeq;

        private int _limitRemain;
        private bool _limitDailyReached;

        public void Bind(ShopOfferDto dto, Action<ShopOfferDto> onClick)
        {
            Unsubscribe();

            _dto = dto;
            _onClick = onClick;

            if (!string.IsNullOrEmpty(dto.Title))
            {
                _header.gameObject.SetActive(true);
                _headerImage.sprite = dto.SpriteHeader;
                _title.SetReference(LocalizationConst.TableUI, dto.Title);
            }
            else
            {
                _header.gameObject.SetActive(false);
            }

            _idleCtaText = BuildIdleCtaText();
            _ctaText.text = _idleCtaText;

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

        public void RefreshLocalizedState()
        {
            if (_dto == null)
                return;

            _idleCtaText = BuildIdleCtaText();

            if (_dto.Type == ShopOfferTypeDto.RewardedAd)
            {
                SyncRewardedState();
                return;
            }

            ApplyInitialState();
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

            if (Time.unscaledTime < _nextTickTime) return;
            _nextTickTime = Time.unscaledTime + 1f;

            SyncRewardedState();
        }

        private void SyncRewardedState()
        {
            if (_ads != null && _ads.IsShowing(_dto.RewardType))
            {
                SetLoading(true);
                return;
            }

            if (_limits != null)
            {
                bool can = _limits.CanClaim(_dto.RewardType, out _limitRemain, out _limitDailyReached);

                if (!can)
                {
                    if (_limitDailyReached)
                    {
                        SetBlockedDaily();
                        return;
                    }

                    if (_limitRemain > 0)
                    {
                        SetCooldown(_limitRemain);
                        return;
                    }
                }
            }

            if (_ads == null)
            {
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

            if (_limits != null)
            {
                bool can = _limits.CanClaim(_dto.RewardType, out _limitRemain, out _limitDailyReached);
                if (!can)
                {
                    if (_limitDailyReached) SetBlockedDaily();
                    else if (_limitRemain > 0) SetCooldown(_limitRemain);
                    else SetLoading(false);
                    return;
                }
            }

            if (_dto.Type == ShopOfferTypeDto.RewardedAd && _ads != null && _ads.IsShowing(_dto.RewardType))
                return;

            if (_dto.Type == ShopOfferTypeDto.RewardedAd)
                SetLoading(true);

            SendAnalytics();

            _onClick?.Invoke(_dto);
        }

        private void OnAdAvailabilityChanged(RewardType type, bool isReady)
        {
            if (_dto == null) return;
            if (_dto.Type != ShopOfferTypeDto.RewardedAd) return;
            if (_dto.RewardType != type) return;

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

            SyncRewardedState();
        }

        private void SetReady()
        {
            _limitDailyReached = false;
            _limitRemain = 0;
            _buyButton.interactable = true;
            _ctaText.text = _idleCtaText;
        }

        private void SetBlockedDaily()
        {
            _limitDailyReached = true;
            _buyButton.interactable = false;
            _ctaText.text = _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyTextLimit);
        }

        private void SetCooldown(int remainSeconds)
        {
            _limitRemain = remainSeconds;
            _buyButton.interactable = false;
            _ctaText.text = _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyTextThrough, FormatMMSS(remainSeconds));
        }

        private void SetLoading(bool showDots)
        {
            _limitDailyReached = false;
            _limitRemain = 0;
            _buyButton.interactable = false;
            _ctaText.text = showDots ? "..." : _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyLabelLoading);
        }

        private string BuildIdleCtaText()
        {
            if (_dto == null)
                return "-";

            if (_dto.Type == ShopOfferTypeDto.RewardedAd)
                return _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyTextLook);

            return string.IsNullOrWhiteSpace(_dto.CtaText) ? "-" : _dto.CtaText;
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

        private void SendAnalytics()
        {
            var eventName = _dto.Type switch
            {
                ShopOfferTypeDto.IapPack => _dto.IsDisableInterstitialAds ? AnalyticsEvents.Monetization.RemoveAdOfferShopClicked : AnalyticsEvents.Monetization.IapOfferShopClicked,
                ShopOfferTypeDto.RewardedAd => AnalyticsEvents.Monetization.AdOfferShopClicked,
                _ => AnalyticsEvents.Monetization.RemoveAdOfferShopClicked
            };

            Dictionary<string, object> parameters = null;
            switch (eventName)
            {
                case AnalyticsEvents.Monetization.IapOfferShopClicked:
                    parameters = new()
                    {
                        [AnalyticsEvents.Parameter.ProductId] = _dto.ProductId,
                        [AnalyticsEvents.Parameter.Reward] = AnalyticsPayloadHelper.GetRewardsPayload(_dto.Rewards),
                        [AnalyticsEvents.Parameter.Price] = _dto.CtaText,
                    };
                    break;
                case AnalyticsEvents.Monetization.AdOfferShopClicked:
                    var limits = _limits?.GetSnapshot(_dto.RewardType) ?? new RewardedLimitSnapshot(0, 0, 0, 0, false);
                    bool willShowLimitState = limits.DailyLimit > 0 && limits.UsedToday + 1 >= limits.DailyLimit;
                    string result = AnalyticsEvents.Option.Success;
                    if (limits.DailyLimitReached || willShowLimitState)
                        result = AnalyticsEvents.Option.Limit;
                    else if (limits.CooldownSeconds > 0)
                        result = AnalyticsEvents.Option.Cooldown;

                    parameters = new()
                    {
                        [AnalyticsEvents.Parameter.Reward] = AnalyticsPayloadHelper.GetRewardsPayload(_dto.Rewards),
                        [AnalyticsEvents.Parameter.LimitRemain] = FormatLimitRemain(limits),
                        [AnalyticsEvents.Parameter.Result] = result,
                    };
                    break;
                case AnalyticsEvents.Monetization.RemoveAdOfferShopClicked:
                    parameters = new()
                    {
                        [AnalyticsEvents.Parameter.Price] = _dto.CtaText,
                    };
                    break;
            }

            _analytics.TrackEvent(eventName, parameters);
        }

        private static string FormatLimitRemain(RewardedLimitSnapshot limits)
        {
            if (limits.DailyLimit <= 0)
                return "0/0";

            int currentAttempt = limits.DailyLimitReached
                ? limits.DailyLimit
                : Mathf.Clamp(limits.UsedToday + 1, 1, limits.DailyLimit);

            return $"{currentAttempt}/{limits.DailyLimit}";
        }
    }
}
