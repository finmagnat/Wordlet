using System;
using Core.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Popups
{
    public sealed class DailyBonusPackItemView : MonoBehaviour
    {
        [Inject] private LocalizationService _localization;
        [Inject] private AnalyticsService _analytics;
        [Inject] private ConfigService _configService;
        
        [SerializeField] private DailyBonusItemView _itemPrefab;
        [SerializeField] private Transform _contentRoot;
        [SerializeField] private Image _headerImage;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private Button _takeButton;
        
        private Action _onClick;

        public void Bind(DailyBonusCycleDay dayData, bool isActiveReward, Action onClick)
        {
            _onClick = onClick;
            var bonusVisualConfig = _configService.DailyBonusVisual;
            
            if (isActiveReward)
            {
                // активный header, кнопка "Забрать"
                _headerImage.sprite = bonusVisualConfig.activeRewardHeader;
                _title.text = _localization.Get(LocalizationConst.TableUI, bonusVisualConfig.activeRewardTitle);
                _takeButton.gameObject.SetActive(true);
            }
            else
            {
                // обычный день
                _headerImage.sprite = bonusVisualConfig.dayNumberHeader;
                _title.text = _localization.Get(LocalizationConst.TableUI, bonusVisualConfig.dayNumberTitle, dayData.Day);
                _takeButton.gameObject.SetActive(false);
            }

            for (int i = _contentRoot.childCount - 1; i >= 0; i--)
                Destroy(_contentRoot.GetChild(i).gameObject);
            
            if (dayData.IsChest)
            {
                var view = Instantiate(_itemPrefab, _contentRoot);
                view.iconImage.sprite = bonusVisualConfig.chestSprite;
                view.counter.SetActive(false);
            }
            else
            {
                foreach (var item in dayData.Rewards)
                {
                    var view = Instantiate(_itemPrefab, _contentRoot);
                   
                    view.countText.text = item.Amount.ToString();
                    var rewardItem = bonusVisualConfig.GetItemByType(item.BoosterType);
                    if (rewardItem != null)
                        view.iconImage.sprite = rewardItem.iconImage;
                }
            }

            _takeButton.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            _onClick?.Invoke();
        }

        private void OnDestroy()
        {
            _takeButton.onClick.RemoveAllListeners();
        }
    }
}
