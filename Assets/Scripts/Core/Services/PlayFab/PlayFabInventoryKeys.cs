using Inventory;

namespace Core.Services
{
    public class PlayFabInventoryKeys
    {
        public static string ToKey(BoosterType type) => RewardedBoosterCatalog.GetPlayFabKey(type);
    }
}
