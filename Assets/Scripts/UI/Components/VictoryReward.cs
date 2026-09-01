using Core.Data;
using Core.Services.Shop;
using UnityEngine;
using Zenject;

namespace Core.UI.Components
{
    public class VictoryReward : MonoBehaviour
    {
        [SerializeField] private ShopItemView _itemPrefab;
        [SerializeField] private Transform _contentRoot;

        [Inject] private DiContainer _container;
        
        public void SetData(FinishGamePopupData data)
        {
            for (int i = _contentRoot.childCount - 1; i >= 0; i--)
                Destroy(_contentRoot.GetChild(i).gameObject);
            
            foreach (var item in data.Reward.Rewards)
            {
                var view = _container.InstantiatePrefabForComponent<ShopItemView>(_itemPrefab, _contentRoot);
                view.Bind(item);
            }
        }
    }
}