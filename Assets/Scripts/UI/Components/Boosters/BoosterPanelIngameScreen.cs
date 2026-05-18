using Core.Config;

namespace UI.Components
{
    public class BoosterPanelIngameScreen : BoosterPanelUI
    {
        private BoosterUI _boosterSlowdown;
        
        public void SlowdownStart()
        {
            var booster = GetSlowdown();
            if (!booster.IsActive)
                booster.ActivateBooster();
        }
        
        public void SlowdownStop()
        {
            var booster = GetSlowdown();
            if (booster.IsActive)
                booster.Cancel();
        }
        
        public bool IsActive(BoosterType boosterType)
        {
            foreach (var booster in _boosters)
                if (booster.Type == boosterType && booster.IsActive)
                    return true;
            return false;
        }
        
        private BoosterUI GetSlowdown()
        {
            if (_boosterSlowdown == null)
            {
                foreach (var booster in _boosters)
                    if (booster.Type == BoosterType.Slowdown)
                        _boosterSlowdown = booster;
            }
            return _boosterSlowdown;
        }

    }
}