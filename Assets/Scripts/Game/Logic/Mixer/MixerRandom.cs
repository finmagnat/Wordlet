using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic.Mixer
{
    internal static class MixerRandom
    {
        public static int Range(int minInclusive, int maxExclusive)
        {
            return Random.Range(minInclusive, maxExclusive);
        }

        public static T Pick<T>(IReadOnlyList<T> items)
        {
            return items[Range(0, items.Count)];
        }

        public static void Shuffle<T>(IList<T> items)
        {
            for (int i = items.Count - 1; i > 0; --i)
            {
                int randomIndex = Range(0, i + 1);
                Swap(items, i, randomIndex);
            }
        }

        public static void Swap<T>(IList<T> items, int firstIndex, int secondIndex)
        {
            if (firstIndex == secondIndex)
                return;

            T temp = items[firstIndex];
            items[firstIndex] = items[secondIndex];
            items[secondIndex] = temp;
        }
    }
}
