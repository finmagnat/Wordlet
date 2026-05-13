using System.Collections.Generic;
using Core.Data;
using Core.Services;
using Core.Services.Shop;
using Cysharp.Threading.Tasks;
using Inventory;
using UnityEngine;
using Zenject;

namespace UI.Popups
{
    public class WinPopup : FinishGamePopup
    {
        [Header("Reward Elements")]
        [SerializeField] private ShopItemView _itemPrefab;
        [SerializeField] private Transform _contentRoot;

        [Inject] private IInventoryService _inventory;
        
        public override async UniTask PrepareAsync(FinishGamePopupData data)
        {
            await base.PrepareAsync(data);
            
            for (int i = _contentRoot.childCount - 1; i >= 0; i--)
                Destroy(_contentRoot.GetChild(i).gameObject);
            
            foreach (var item in data.Rewards)
            {
                var view = Instantiate(_itemPrefab, _contentRoot);
                view.Bind(item);
            }

            await UniTask.CompletedTask;
        }

        protected override Dictionary<string, object> GetAnalyticsParams()
        {
            var dictionary = base.GetAnalyticsParams();
            dictionary[AnalyticsEvents.Parameter.Reward] = AnalyticsPayloadHelper.GetRewardsPayload(_data.Rewards);
            dictionary[AnalyticsEvents.Parameter.Boosters] = AnalyticsPayloadHelper.GetBoostersPayload(_inventory.Boosters);
            return dictionary;
        }
    }
}
