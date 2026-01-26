using System;
using Inventory;

namespace Core.Services
{
    public class PlayFabInventoryKeys
    {
        public const string BoostLetter = "boost_letter";
        public const string BoostSlow   = "boost_slow";

        public static string ToKey(BoosterType type) => type switch
        {
            BoosterType.Letter   => BoostLetter,
            BoosterType.Slowdown => BoostSlow,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}