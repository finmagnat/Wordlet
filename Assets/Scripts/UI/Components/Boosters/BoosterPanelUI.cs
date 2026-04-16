using Inventory;
using UnityEngine;
using Zenject;

namespace UI.Components
{
    public class BoosterPanelUI : MonoBehaviour
    {
        [SerializeField] protected BoosterUI boosterLetter;
        [SerializeField] protected BoosterUI boosterSlowdown;
        [SerializeField] protected BoosterUI boosterEraser;

        [Inject] protected IInventoryService _inventory;

        public virtual void Refresh()
        {
            var count = _inventory.GetQuantity(BoosterType.Letter);
            boosterLetter.SetBoosterData(BoosterType.Letter, count);
            
            count = _inventory.GetQuantity(BoosterType.Slowdown);
            boosterSlowdown.SetBoosterData(BoosterType.Slowdown, count);
            
            count = _inventory.GetQuantity(BoosterType.Eraser);
            boosterEraser.SetBoosterData(BoosterType.Eraser, count);
        }
    }
}