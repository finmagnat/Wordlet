using System;

namespace Inventory
{
    [Serializable]
    public class BoosterItem
    {
        public BoosterType Type;
        public int Count;

        public BoosterItem(BoosterType type, int count)
        {
            Type = type;
            Count = count;
        }
    }

}