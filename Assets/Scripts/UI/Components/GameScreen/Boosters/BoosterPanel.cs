using Inventory;
using UnityEngine;
using Zenject;

namespace UI.Components
{
    public class BoosterPanel : MonoBehaviour
    {
        [SerializeField] private BoosterUI boosterLetter;
        [SerializeField] private BoosterUI boosterSlowdown;

        [Inject] private IInventoryService _inventory;

        public void Refresh()
        {
            foreach (var booster in _inventory.Boosters.Values)
            {
                Debug.Log($"{booster.Type}: {booster.Count}");
            }
        }

        public void OnUseLetter()
        {
            Debug.Log("Буковка КЛИК");
            if (!boosterLetter.IsActive && _inventory.TryConsumeBooster(BoosterType.Letter))
            {
                Debug.Log("Буковка использована");
                Refresh();
            }
        }

        public void OnUseSlowdown()
        {
            Debug.Log("Замедлялка КЛИК");
            if (!boosterSlowdown.IsActive && _inventory.TryConsumeBooster(BoosterType.Slowdown))
            {
                Debug.Log("Замедлялка использована");
                Refresh();
            }
        }
    }
}