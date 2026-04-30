using System;
using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Inventory;

namespace Core.Services
{
    public static class AnalyticsPayloadHelper
    {
        public static string GetBoostersPayload(IReadOnlyDictionary<BoosterType, BoosterItem> boosters)
        {
            if (boosters == null || boosters.Count == 0)
                return "[]";

            return BuildItemAmountsJson(
                boosters
                    .Where(x => x.Key != BoosterType.None)
                    .OrderBy(x => x.Key),
                x => x.Value.Type.ToString(),
                x => x.Value.Count);
        }

        public static string GetRewardsPayload(IEnumerable<ShopRewardDto> rewards)
        {
            return BuildItemAmountsJson(
                rewards,
                x => x.ItemId.ToString(),
                x => x.Amount);
        }

        public static string GetFieldPayload(IReadOnlyList<string> boardData)
        {
            if (boardData == null || boardData.Count == 0)
                return "{}";

            return "{"
                   + string.Join(",",
                       boardData.Select((value, index) =>
                           $"\"{index}\":\"{EscapeJson(value ?? string.Empty)}\""))
                   + "}";
        }

        public static string GetIndexedItemPayload(int index, string value)
        {
            return $"{{\"{index}\":\"{EscapeJson(value ?? string.Empty)}\"}}";
        }

        public static string GetIndexesPayload(IEnumerable<int> indexes)
        {
            if (indexes == null)
                return "[]";

            return "[" + string.Join(",", indexes) + "]";
        }

        private static string BuildItemAmountsJson<T>(
            IEnumerable<T> items,
            Func<T, string> getItemId,
            Func<T, int> getAmount)
        {
            if (items == null)
                return "[]";

            return "[" + string.Join(",",
                items.Select(item => $"{{\"item_id\":\"{EscapeJson(getItemId(item))}\",\"amount\":{getAmount(item)}}}")) + "]";
        }

        private static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }
    }
}
