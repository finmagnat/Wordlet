using System.Collections.Generic;
using Core.Data;
using Core.Generated;
using Core.Services;
using Core.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Popups
{
    public class DailyBonusPopup : UIPopup
    {
        [SerializeField] private DailyBonusPackItemView _dayItemPrefab;
        [SerializeField] private Transform _scrollListContent;
        [SerializeField] private Button _closeButton;
        
        [Inject] private IDailyBonusService _dailyBonusService;
        [Inject] private IUIManager _ui;
        [Inject] private ConfigService _configService;
        [Inject] private AudioService _audioService;
        
        [Inject] private DiContainer _container;
        [Inject] private AnalyticsService _analytics;
        
        private readonly List<DailyBonusPackItemView> _dayItems = new();
        private DailyBonusPackItemView _activeDayItem;
        private ScrollRect _scrollRect;
        private bool _isClaiming;

        private void Start()
        {
            _closeButton.onClick.AddListener(async () =>
            {
                SendAnalytics(AnalyticsEvents.Navigation.DailyBonusPopupClickedClose);
                await HideAsync();
            });
        }

        public override async UniTask ShowAsync()
        {
            RebuildDayItems();
            
            SendAnalytics(AnalyticsEvents.Navigation.DailyBonusPopupShown);
            
            await base.ShowAsync();
        }

        protected override async UniTask BeforeShowAnimationAsync()
        {
            await base.BeforeShowAnimationAsync();

            ScrollToActiveReward();
            await UniTask.Yield(PlayerLoopTiming.Update);
            ScrollToActiveReward();
        }
        
        private void OnTakeButtonClick()
        {
            if (_isClaiming)
                return;

            ClaimDailyBonusAsync().Forget();
        }

        private async UniTaskVoid ClaimDailyBonusAsync()
        {
            _isClaiming = true;
            SendAnalytics(AnalyticsEvents.Navigation.DailyBonusPopupClickedTake);

            try
            {
                var result = await _dailyBonusService.TryClaimAsync();
                if (!result.Success)
                {
                    Debug.LogWarning($"Daily bonus reward claim was not granted: {result.Error}");
                    RebuildDayItems();
                    return;
                }

                await HideAsync();
                await _ui.ShowPopupAsync<RewardPopup, RewardPopupData>(AssetKey.RewardPopup, CreateRewardPopupData(result));
            }
            finally
            {
                _isClaiming = false;
            }
        }

        private void RebuildDayItems()
        {
            var state = _dailyBonusService.CurrentState;
            var cycle = _dailyBonusService.CurrentCycle;

            _dayItems.Clear();
            _activeDayItem = null;

            for (int i = _scrollListContent.childCount - 1; i >= 0; i--)
            {
                var child = _scrollListContent.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }

            foreach (var dayItemData in cycle.Days)
            {
                bool isActiveReward =
                    _dailyBonusService.IsAvailable &&
                    state.DailyRewardDay == dayItemData.Day;

                var dayItem = _container.InstantiatePrefabForComponent<DailyBonusPackItemView>(
                    _dayItemPrefab,
                    _scrollListContent);

                dayItem.Bind(dayItemData, isActiveReward, OnTakeButtonClick);

                if (isActiveReward)
                    _activeDayItem = dayItem;

                _dayItems.Add(dayItem);
            }
        }

        private void ScrollToActiveReward()
        {
            var scrollRect = ResolveScrollRect();
            if (scrollRect == null || _activeDayItem == null)
                return;

            var content = scrollRect.content != null
                ? scrollRect.content
                : _scrollListContent as RectTransform;
            var viewport = scrollRect.viewport != null
                ? scrollRect.viewport
                : scrollRect.transform as RectTransform;
            var activeItemRect = _activeDayItem.transform as RectTransform;

            if (content == null || viewport == null || activeItemRect == null)
                return;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);

            float scrollableHeight = content.rect.height - viewport.rect.height;
            if (scrollableHeight <= 0f)
            {
                scrollRect.verticalNormalizedPosition = 1f;
                return;
            }

            Vector3 itemCenterInViewport = viewport.InverseTransformPoint(
                activeItemRect.TransformPoint(activeItemRect.rect.center));
            float targetContentY =
                content.anchoredPosition.y + viewport.rect.center.y - itemCenterInViewport.y;
            float contentY = Mathf.Clamp(targetContentY, 0f, scrollableHeight);

            content.anchoredPosition = new Vector2(content.anchoredPosition.x, contentY);
            scrollRect.verticalNormalizedPosition = 1f - contentY / scrollableHeight;
            scrollRect.velocity = Vector2.zero;
        }

        private ScrollRect ResolveScrollRect()
        {
            if (_scrollRect != null)
                return _scrollRect;

            if (_scrollListContent == null)
                return null;

            _scrollRect = _scrollListContent.GetComponentInParent<ScrollRect>(true);
            return _scrollRect;
        }

        private RewardPopupData CreateRewardPopupData(DailyBonusClaimResult result)
        {
            var rewards = new List<RewardDto>();
            var visualConfig = _configService.DailyBonusVisual;

            foreach (var reward in result.Rewards)
            {
                var visual = visualConfig != null
                    ? visualConfig.GetItemByType(reward.BoosterType)
                    : null;

                rewards.Add(new RewardDto
                {
                    ItemId = reward.BoosterType,
                    Amount = reward.Amount,
                });
            }

            return new RewardPopupData
            {
                Source = RewardPopupData.SourceDailyBonus,
                DailyBonusDay = result.ClaimedDay,
                DailyBonusJackpot = result.IsJackpot,
                Rewards = rewards
            };
        }
        
        private void SendAnalytics(string eventName)
        {
            _analytics.TrackEvent(eventName, CreateAnalyticsParams());
        }

        private Dictionary<string, object> CreateAnalyticsParams()
        {
            var state = _dailyBonusService.CurrentState;
            int day = state?.DailyRewardDay ?? 0;
            var dayConfig = GetCycleDay(day);

            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Day] = day,
                [AnalyticsEvents.Parameter.RewardType] = GetRewardType(dayConfig),
                [AnalyticsEvents.Parameter.Reward] = GetRewardPayload(dayConfig)
            };
        }

        private DailyBonusCycleDay GetCycleDay(int day)
        {
            if (day <= 0)
                return null;

            foreach (var item in _dailyBonusService.CurrentCycle.Days)
            {
                if (item.Day == day)
                    return item;
            }

            return null;
        }

        private static string GetRewardType(DailyBonusCycleDay dayConfig)
        {
            if (dayConfig == null)
                return AnalyticsEvents.Option.Unknown;

            return dayConfig.IsChest
                ? AnalyticsEvents.Option.Chest
                : AnalyticsEvents.Option.Booster;
        }

        private static string GetRewardPayload(DailyBonusCycleDay dayConfig)
        {
            if (dayConfig == null)
                return AnalyticsEvents.Option.Unknown;

            if (dayConfig.IsChest)
                return AnalyticsEvents.Option.Chest;

            var rewards = new List<RewardDto>();
            foreach (var reward in dayConfig.Rewards)
            {
                rewards.Add(new RewardDto
                {
                    ItemId = reward.BoosterType,
                    Amount = reward.Amount
                });
            }

            return AnalyticsPayloadHelper.GetRewardsPayload(rewards);
        }
    }
}
