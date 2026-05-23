using System.Collections.Generic;
using Core.Services;
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
        [Inject] private AudioService _audioService;
        
        [Inject] private DiContainer _container;
        [Inject] private AnalyticsService _analytics;
        
        private readonly List<DailyBonusPackItemView> _dayItems = new();

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
            var state = _dailyBonusService.CurrentState;
            var cycle = _dailyBonusService.CurrentCycle;
            
            if (_dayItems == null || _dayItems.Count == 0)
            {
                for (int i = _scrollListContent.childCount - 1; i >= 0; i--)
                    Destroy(_scrollListContent.GetChild(i).gameObject);
                
                foreach (var dayItemData in _dailyBonusService.CurrentCycle.Days)
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
            
            SendAnalytics(AnalyticsEvents.Navigation.DailyBonusPopupShown);
            
            await base.ShowAsync();
        }
        
        private void OnTakeButtonClick()
        {
            SendAnalytics(AnalyticsEvents.Navigation.DailyBonusPopupClickedTake);
        }
        
        private void SendAnalytics(string eventName)
        {
            Dictionary<string, object> parameters = null;
            switch (eventName)
            {
                case AnalyticsEvents.Navigation.DailyBonusPopupShown:
                case AnalyticsEvents.Navigation.DailyBonusPopupClickedClose:
                    parameters = new Dictionary<string, object>
                    {
                        //[AnalyticsEvents.Parameter.] = ,
                        //[AnalyticsEvents.Parameter.] = ,
                    };
                    break;
            }
            
            _analytics.TrackEvent(eventName, parameters);
        }
    }
}