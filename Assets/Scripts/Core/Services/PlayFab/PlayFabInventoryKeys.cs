using Core.Config;

namespace Core.Services
{
    public class PlayFabInventoryKeys
    {
        public static string ToKey(BoosterType type) => RewardedBoosterCatalog.GetPlayFabKey(type);
    }
}
