using Core.Events;
using Core.Services;
using Cysharp.Threading.Tasks;
using TMPro;
using UI.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Components
{
    public class StatisticsPanel : UIPopup
    {
        [SerializeField] protected Button _closeButton;
        [SerializeField] private Image _mainBackground;
        [SerializeField] protected TextMeshProUGUI _startWordText;
        [SerializeField] protected StatisticPlayerPanel _statisticPlayerPlayerPanelOwner;
        [SerializeField] protected StatisticPlayerPanel _statisticPlayerPlayerPanelOpponent;
     
        internal StatisticPlayerPanel StatisticPlayerPlayerPanelOwner => _statisticPlayerPlayerPanelOwner;
        internal StatisticPlayerPanel StatisticPlayerPlayerPanelOpponent => _statisticPlayerPlayerPanelOpponent;
        
        [Inject] private SkinsService _skinsService;
        [Inject] private ISpriteService _spritesService;
        [Inject] private LocalizationService _localization;
        [Inject] private AnalyticsService _analytics;
        
        private string _startWord;
        
        private void Start()
        {
            _closeButton.onClick.AddListener(async () =>
            {
                _analytics.TrackEvent(AnalyticsEvents.GameFlow.CloseHistoryGameClicked);
                await HideAsync();
            });
        }

        internal void SetStartWord(string value)
        {
            _startWord = value;
            _startWordText.text = $"{value}   <size=100%><voffset=20><sprite name=\"magnifier\"></voffset></size>";
        }

        public void OnStartWordPressed() => EventBus.Raise(new ShowWordInfoEvent{word = _startWord});
        
        internal async UniTask UpdateSkin()
        {
            var skin = _skinsService.SkinCurrent;
            _mainBackground.sprite = await _spritesService.GetSpriteAsync(skin.MainBackgroundAlias);
            _statisticPlayerPlayerPanelOwner.UpdateSkin();
            _statisticPlayerPlayerPanelOpponent.UpdateSkin();
        }

        internal void Reset()
        {
            _startWordText.text = "";
            _statisticPlayerPlayerPanelOwner.Reset();
            _statisticPlayerPlayerPanelOpponent.Reset();
        }
    }
}
