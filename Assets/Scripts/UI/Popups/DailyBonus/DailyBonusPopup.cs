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

            for (int i = _scrollListContent.childCount - 1; i >= 0; i--)
                Destroy(_scrollListContent.GetChild(i).gameObject);

            foreach (var dayItemData in cycle.Days)
            {
                bool isActiveReward =
                    _dailyBonusService.IsAvailable &&
                    state.DailyRewardDay == dayItemData.Day;

                var dayItem = _container.InstantiatePrefabForComponent<DailyBonusPackItemView>(
                    _dayItemPrefab,
                    _scrollListContent);

                dayItem.Bind(dayItemData, isActiveReward, OnTakeButtonClick);

                _dayItems.Add(dayItem);
            }
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
                    SpriteIcon = visual?.iconImage
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
