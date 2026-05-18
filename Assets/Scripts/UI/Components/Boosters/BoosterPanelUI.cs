using Core.Services.Common;
using Inventory;
using UnityEngine;
using Zenject;

namespace UI.Components
{
    public class BoosterPanelUI : MonoBehaviour
    {
        [SerializeField] protected BoosterUI[] _boosters;
        
        [Inject] protected IInventoryService _inventory;

        public virtual void Refresh()
        {
            foreach (var booster in _boosters)
            {
                if(booster.Type == BoosterType.Mixer)
                    booster.SetBoosterData(new BoosterItem(BoosterType.Mixer, 0, isInfinite: true));
                else
                    booster.SetBoosterData(_inventory.GetItem(booster.Type));
            }
        }
        
        public bool IsActive(BoosterType boosterType)
        {
            foreach (var booster in _boosters)
                if (booster.Type == boosterType && booster.IsActive)
                    return true;
            return false;
        }
    }
}
