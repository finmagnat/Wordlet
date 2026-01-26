using System.Collections.Generic;
using Core.Services;

namespace Inventory
{
    public interface IInventoryService : IService
    {
        IReadOnlyDictionary<BoosterType, BoosterItem> Boosters { get; }

        bool HasBooster(BoosterType type);
        int GetQuantity(BoosterType type);
        bool TryConsumeBooster(BoosterType type);

        void SetQuantity(BoosterType type, int count, bool bAdd = false);

        void Add(BoosterType type, int count);
    }

}