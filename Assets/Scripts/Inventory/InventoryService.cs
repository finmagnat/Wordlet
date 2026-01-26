using System.Collections.Generic;

namespace Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly Dictionary<BoosterType, BoosterItem> _boosters = new ();

        public IReadOnlyDictionary<BoosterType, BoosterItem> Boosters => _boosters;

        public bool HasBooster(BoosterType type)
        {
            return _boosters.TryGetValue(type, out var item) && item.Count > 0;
        }
        
        public int GetQuantity(BoosterType type)
        {
            if (!HasBooster(type))
                return 0;
            
            return _boosters[type].Count;
        }
        
        public bool TryConsumeBooster(BoosterType type)
        {
            if (!HasBooster(type))
                return false;

            _boosters[type].Count--;
            return true;
        }

        public void SetQuantity(BoosterType type, int count, bool bAdd = false)
        {
            if (_boosters.ContainsKey(type))
                _boosters[type].Count = bAdd ? _boosters[type].Count + count : count;
            else
                _boosters[type] = new BoosterItem(type, count);
        }
        
        public void Add(BoosterType type, int count)
        {
            SetQuantity(type, count, true);
        }
        
    }

}