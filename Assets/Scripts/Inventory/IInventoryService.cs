using System.Collections.Generic;
using Core.Services;

namespace Inventory
{
    public interface IInventoryService : IService
    {
        IReadOnlyDictionary<BoosterType, BoosterItem> Boosters { get; }

        bool HasBooster(BoosterType type);
        bool TryConsumeBooster(BoosterType type);

        void SetBoosterCount(BoosterType type, int count);
    }

}