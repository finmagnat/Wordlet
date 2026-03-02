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
        
        private void Start()
        {
            _closeButton.onClick.AddListener(async () =>
            {
                await HideAsync();
            });
            //SetSkin();
        }
        
        internal void SetStartWord(string value) => _startWordText.text = value;
        
        private async UniTask SetSkin()
        {
            var skin = _skinsService.SkinCurrent;
            _mainBackground.sprite = await _spritesService.GetSpriteAsync(skin.ListBackgroundAlias);
        }

        internal void Reset()
        {
            _startWordText.text = "";
            _statisticPlayerPlayerPanelOwner.Reset();
            _statisticPlayerPlayerPanelOpponent.Reset();
        }
    }
}