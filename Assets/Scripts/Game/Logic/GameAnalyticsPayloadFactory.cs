using System.Collections.Generic;
using Core.Config;
using Core.Services;
using Inventory;
using UnityEngine;

namespace Game.Logic
{
    public sealed class GameAnalyticsPayloadFactory
    {
        public Dictionary<string, object> CreateAiGameStartedPayload(
            ComplexityAI complexityAI,
            int durationRound,
            IReadOnlyDictionary<BoosterType, BoosterItem> boosters,
            string locale,
            string startWord)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.ComplexityAi] = complexityAI.ToString(),
                [AnalyticsEvents.Parameter.DurationRound] = durationRound,
                [AnalyticsEvents.Parameter.Boosters] = AnalyticsPayloadHelper.GetBoostersPayload(boosters),
                [AnalyticsEvents.Parameter.Locale] = locale,
                [AnalyticsEvents.Parameter.StartWord] = startWord
            };
        }

        public Dictionary<string, object> CreateAiSavedGameStartedPayload(
            string savedGameJson,
            IReadOnlyDictionary<BoosterType, BoosterItem> boosters)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.SavedGame] = string.IsNullOrEmpty(savedGameJson) ? "{}" : savedGameJson,
                [AnalyticsEvents.Parameter.Boosters] = AnalyticsPayloadHelper.GetBoostersPayload(boosters)
            };
        }

        public Dictionary<string, object> CreateGameSnapshotPayload(
            string[] boardData,
            ComplexityAI complexityAI,
            int durationRound,
            float currentTimerValue,
            uint score,
            uint scoreOpponent,
            uint pass,
            uint passOpponent,
            uint maxPasses,
            IReadOnlyDictionary<BoosterType, BoosterItem> boosters)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Boosters] = AnalyticsPayloadHelper.GetBoostersPayload(boosters),
                [AnalyticsEvents.Parameter.ComplexityAi] = complexityAI.ToString(),
                [AnalyticsEvents.Parameter.DurationRound] = durationRound,
                [AnalyticsEvents.Parameter.DurationRoundLeft] = Mathf.Max(0, durationRound - Mathf.RoundToInt(currentTimerValue)),
                [AnalyticsEvents.Parameter.CellsEmpty] = CountEmptyCells(boardData),
                [AnalyticsEvents.Parameter.Score] = (int)score,
                [AnalyticsEvents.Parameter.ScoreOpponent] = (int)scoreOpponent,
                [AnalyticsEvents.Parameter.Pass] = $"{pass}/{maxPasses}",
                [AnalyticsEvents.Parameter.PassOpponent] = $"{passOpponent}/{maxPasses}"
            };
        }

        public Dictionary<string, object> CreateFinishGamePayload(
            Dictionary<string, object> snapshot,
            ResultGame resultGame,
            bool isSavedGame)
        {
            snapshot[AnalyticsEvents.Parameter.Result] = resultGame switch
            {
                ResultGame.OWNER_WIN => AnalyticsEvents.Option.Win,
                ResultGame.OWNER_LOSE => AnalyticsEvents.Option.Lose,
                _ => AnalyticsEvents.Option.Draft
            };

            if (isSavedGame)
                snapshot[AnalyticsEvents.Parameter.SavedGameRemoved] = true;

            return snapshot;
        }

        public Dictionary<string, object> CreateLetterBoosterSuccessPayload(
            string word,
            string locale,
            string[] boardData)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Word] = word,
                [AnalyticsEvents.Parameter.WordLength] = word.Length,
                [AnalyticsEvents.Parameter.Locale] = locale,
                [AnalyticsEvents.Parameter.CellsEmpty] = CountEmptyCells(boardData),
                [AnalyticsEvents.Parameter.Field] = AnalyticsPayloadHelper.GetFieldPayload(boardData)
            };
        }

        public Dictionary<string, object> CreateLetterBoosterFailPayload(
            string locale,
            int durationRound,
            float currentTimerValue,
            string[] boardData)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Locale] = locale,
                [AnalyticsEvents.Parameter.DurationRound] = durationRound,
                [AnalyticsEvents.Parameter.DurationRoundLeft] = Mathf.Max(0, durationRound - Mathf.RoundToInt(currentTimerValue)),
                [AnalyticsEvents.Parameter.CellsEmpty] = CountEmptyCells(boardData),
                [AnalyticsEvents.Parameter.Field] = AnalyticsPayloadHelper.GetFieldPayload(boardData)
            };
        }

        public Dictionary<string, object> CreateEraserBoosterShownPayload(
            string locale,
            string[] boardData)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Locale] = locale,
                [AnalyticsEvents.Parameter.Field] = AnalyticsPayloadHelper.GetFieldPayload(boardData)
            };
        }

        public Dictionary<string, object> CreateEraserBoosterClosedPayload(
            int durationRound,
            float currentTimerValue,
            IReadOnlyDictionary<BoosterType, BoosterItem> boosters)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.DurationRound] = durationRound,
                [AnalyticsEvents.Parameter.DurationRoundLeft] = Mathf.Max(0, durationRound - Mathf.RoundToInt(currentTimerValue)),
                [AnalyticsEvents.Parameter.Boosters] = AnalyticsPayloadHelper.GetBoostersPayload(boosters)
            };
        }

        public Dictionary<string, object> CreateEraserBoosterSuccessPayload(
            string locale,
            string[] boardData,
            int erasedIndex,
            string erasedLetter,
            int durationRound,
            float currentTimerValue,
            IReadOnlyDictionary<BoosterType, BoosterItem> boosters)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Locale] = locale,
                [AnalyticsEvents.Parameter.Field] = AnalyticsPayloadHelper.GetFieldPayload(boardData),
                [AnalyticsEvents.Parameter.EraseItem] = AnalyticsPayloadHelper.GetIndexedItemPayload(erasedIndex, erasedLetter),
                [AnalyticsEvents.Parameter.DurationRound] = durationRound,
                [AnalyticsEvents.Parameter.DurationRoundLeft] = Mathf.Max(0, durationRound - Mathf.RoundToInt(currentTimerValue)),
                [AnalyticsEvents.Parameter.Boosters] = AnalyticsPayloadHelper.GetBoostersPayload(boosters)
            };
        }

        public Dictionary<string, object> CreateSlowdownBoosterSuccessPayload(
            int slowdownDelay,
            string locale,
            int durationRound,
            float currentTimerValue,
            string[] boardData,
            IReadOnlyDictionary<BoosterType, BoosterItem> boosters)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.SlowdownDelay] = slowdownDelay,
                [AnalyticsEvents.Parameter.Locale] = locale,
                [AnalyticsEvents.Parameter.DurationRound] = durationRound,
                [AnalyticsEvents.Parameter.DurationRoundLeft] = Mathf.Max(0, durationRound - Mathf.RoundToInt(currentTimerValue)),
                [AnalyticsEvents.Parameter.Field] = AnalyticsPayloadHelper.GetFieldPayload(boardData),
                [AnalyticsEvents.Parameter.Boosters] = AnalyticsPayloadHelper.GetBoostersPayload(boosters)
            };
        }

        private static int CountEmptyCells(IReadOnlyList<string> boardData)
        {
            if (boardData == null)
                return 0;

            int emptyCells = 0;
            for (int i = 0; i < boardData.Count; i++)
            {
                if (string.IsNullOrEmpty(boardData[i]))
                    emptyCells++;
            }

            return emptyCells;
        }
    }
}
