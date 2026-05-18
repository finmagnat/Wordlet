using System;
using Core.Services.Common;

namespace Inventory
{
    [Serializable]
    public class BoosterItem
    {
        public BoosterType Type;
        public int Count;
        public bool IsInfinite;

        public BoosterItem(BoosterType type, int count, bool isInfinite =  false)
        {
            Type = type;
            Count = count;
            IsInfinite = isInfinite;
        }
    }

}