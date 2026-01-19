using Inventory;
using UnityEngine;
using Zenject;

namespace UI.Components
{
    public class BoosterPanelUI : MonoBehaviour
    {
        [SerializeField] protected BoosterUI boosterLetter;
        [SerializeField] protected BoosterUI boosterSlowdown;

        [Inject] protected IInventoryService _inventory;

        public virtual void Refresh()
        {
            var count = _inventory.GetCount(BoosterType.Letter);
            boosterLetter.SetBoosterData(BoosterType.Letter, count);
            
            count = _inventory.GetCount(BoosterType.Slowdown);
            boosterSlowdown.SetBoosterData(BoosterType.Slowdown, count);
        }
    }
}