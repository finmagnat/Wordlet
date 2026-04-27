using Core.Events;
using Inventory;
using UnityEngine;

namespace UI.Components
{
    public class BoosterPanelIngameScreen : BoosterPanelUI
    {
        public void OnUseLetter() => UseBoosterHandler(boosterLetter);

        public void OnUseSlowdown() => UseBoosterHandler(boosterSlowdown);
        
        public void OnUseEraser()  => UseBoosterHandler(boosterEraser);
        
        public void OnUseMixer()  => UseBoosterHandler(boosterMixer);
        
        private void UseBoosterHandler(BoosterUI boosterUI)
        {
            if (!boosterUI.IsActive)
            {
                Debug.Log($"Использовать {boosterUI.Type}, IsEmpty = {boosterUI.IsEmpty}");
                EventBus.Raise(new UseBoosterEvent{ boosterType = boosterUI.Type, isEmpty = boosterUI.IsEmpty});
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