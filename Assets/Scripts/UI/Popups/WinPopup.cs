using System.Collections.Generic;
using Core.Data;
using Core.Services;
using Core.UI.Components;
using Cysharp.Threading.Tasks;
using Inventory;
using UnityEngine;
using Zenject;

namespace UI.Popups
{
    public class WinPopup : FinishGamePopup
    {
        [Header("Reward Elements")]
        
        [SerializeField] private VictoryReward _victoryReward;
        [SerializeField] private ProgressVictories _progressVictories;

        [Inject] private IInventoryService _inventory;
        
        public override async UniTask PrepareAsync(FinishGamePopupData data)
        {
            await base.PrepareAsync(data);

            if (_data.Reward.Rewards != null)
            {
                _progressVictories.gameObject.SetActive(false);
                _victoryReward.gameObject.SetActive(true);
                _victoryReward.SetData(data);
            }
            else
            {
                _progressVictories.gameObject.SetActive(true);
                _victoryReward.gameObject.SetActive(false);
                _progressVictories.SetData(data);
            }

            await UniTask.CompletedTask;
        }

        protected override Dictionary<string, object> GetAnalyticsParams()
        {
            var dictionary = base.GetAnalyticsParams();
            
            dictionary[AnalyticsEvents.Parameter.WinsInSeriesCount] = _data.Reward.WinsInSeriesCount;
            dictionary[AnalyticsEvents.Parameter.WinsInSeriesMax] = _data.Reward.WinsInSeriesMax;
            if (_data.Reward.Rewards != null)
            {
                dictionary[AnalyticsEvents.Parameter.Reward] =
                    AnalyticsPayloadHelper.GetRewardsPayload(_data.Reward.Rewards);
                dictionary[AnalyticsEvents.Parameter.Boosters] =
                    AnalyticsPayloadHelper.GetBoostersPayload(_inventory.Boosters);
            }

            return dictionary;
        }
    }
}
