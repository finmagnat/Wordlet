using Core.Events;
using Inventory;
using UnityEngine;

namespace UI.Components
{
    public class BoosterPanelIngameScreen : BoosterPanelUI
    {
        public void OnUseLetter()
        {
            //Debug.Log("Буковка КЛИК");
            if (!boosterLetter.IsActive && !boosterLetter.IsEmpty)
            {
                Debug.Log("Использовать Буковку");
                EventBus.Raise(new UseBoosterEvent{ boosterType = BoosterType.Letter });
            }
        }

        public void OnUseSlowdown()
        {
            //Debug.Log("Замедлялка КЛИК");
            if (!boosterSlowdown.IsActive && !boosterSlowdown.IsEmpty)
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