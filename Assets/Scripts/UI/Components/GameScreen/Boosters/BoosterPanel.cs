using Core.Events;
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
            var count = _inventory.GetCount(BoosterType.Letter);
            boosterLetter.SetBoosterCount(count);
            boosterLetter.gameObject.SetActive(count > 0);
            
            count = _inventory.GetCount(BoosterType.Slowdown);
            boosterSlowdown.SetBoosterCount(count);
            boosterSlowdown.gameObject.SetActive(count > 0);
        }

        public void OnUseLetter()
        {
            //Debug.Log("Буковка КЛИК");
            if (!boosterLetter.IsActive)
            {
                Debug.Log("Использовать Буковку");
                EventBus.Raise(new UseBoosterEvent{ boosterType = BoosterType.Letter });
            }
        }

        public void OnUseSlowdown()
        {
            //Debug.Log("Замедлялка КЛИК");
            if (!boosterSlowdown.IsActive)
            {
                Debug.Log("Использовать Замедлялку");
                EventBus.Raise(new UseBoosterEvent{ boosterType = BoosterType.Slowdown });
            }
        }
        
        public void SlowdownStart()
        {
            if (!boosterSlowdown.IsActive)
                boosterSlowdown.ActivateBooster();
        }
        
        public void SlowdownStop()
        {
            if (boosterSlowdown.IsActive)
                boosterSlowdown.Cancel();
        }
        
        public bool IsActive(BoosterType boosterType)
        {
            switch (boosterType)
            {
                case BoosterType.Letter: return boosterLetter.IsActive;
                case BoosterType.Slowdown: return boosterSlowdown.IsActive;
            }
            return false;
        }
    }
}