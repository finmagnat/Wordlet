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

        public bool TryConsumeBooster(BoosterType type)
        {
            if (!HasBooster(type))
                return false;

            _boosters[type].Count--;
            return true;
        }

        public void SetBoosterCount(BoosterType type, int count)
        {
            if (_boosters.ContainsKey(type))
                _boosters[type].Count = count;
            else
                _boosters[type] = new BoosterItem(type, count);
        }
    }

}