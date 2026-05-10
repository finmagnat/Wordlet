using System.Collections.Generic;
using Core.Config;
using Core.Data;
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
            SaveGameData gameData,
            uint maxPasses,
            IReadOnlyDictionary<BoosterType, BoosterItem> boosters)
        {
            return CreateGameSnapshotPayload(gameData, maxPasses, boosters);
        }

        public Dictionary<string, object> CreateGameSnapshotPayload(
            string locale,
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
            var snapshot = CreateGameplayStatePayload(locale, boardData, durationRound, currentTimerValue, boosters);
            snapshot[AnalyticsEvents.Parameter.ComplexityAi] = complexityAI.ToString();
            snapshot[AnalyticsEvents.Parameter.Score] = (int)score;
            snapshot[AnalyticsEvents.Parameter.ScoreOpponent] = (int)scoreOpponent;
            snapshot[AnalyticsEvents.Parameter.Pass] = $"{pass}/{maxPasses}";
            snapshot[AnalyticsEvents.Parameter.PassOpponent] = $"{passOpponent}/{maxPasses}";

            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Locale] = snapshot[AnalyticsEvents.Parameter.Locale],
                [AnalyticsEvents.Parameter.ComplexityAi] = snapshot[AnalyticsEvents.Parameter.ComplexityAi],
                [AnalyticsEvents.Parameter.DurationRound] = snapshot[AnalyticsEvents.Parameter.DurationRound],
                [AnalyticsEvents.Parameter.DurationRoundLeft] = snapshot[AnalyticsEvents.Parameter.DurationRoundLeft],
                [AnalyticsEvents.Parameter.Score] = snapshot[AnalyticsEvents.Parameter.Score],
                [AnalyticsEvents.Parameter.ScoreOpponent] = snapshot[AnalyticsEvents.Parameter.ScoreOpponent],
                [AnalyticsEvents.Parameter.Pass] = snapshot[AnalyticsEvents.Parameter.Pass],
                [AnalyticsEvents.Parameter.PassOpponent] = snapshot[AnalyticsEvents.Parameter.PassOpponent],
                [AnalyticsEvents.Parameter.CellsEmpty] = snapshot[AnalyticsEvents.Parameter.CellsEmpty],
                [AnalyticsEvents.Parameter.Field] = snapshot[AnalyticsEvents.Parameter.Field],
                [AnalyticsEvents.Parameter.Boosters] = snapshot[AnalyticsEvents.Parameter.Boosters]
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

        public Dictionary<string, object> CreateGameSnapshotPayload(
            SaveGameData gameData,
            uint maxPasses,
            IReadOnlyDictionary<BoosterType, BoosterItem> boosters)
        {
            if (gameData == null)
                return new Dictionary<string, object>();

            return CreateGameSnapshotPayload(
                gameData.localeCode,
                gameData.boardRows,
                (ComplexityAI)gameData.levelComplexityAI,
                gameData.maxSeconds,
                gameData.currentSeconds,
                gameData.playerScore,
                gameData.opponentScore,
                gameData.playerPasses,
                gameData.opponentPasses,
                maxPasses,
                boosters);
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
            var statePayload = CreateGameplayStatePayload(locale, boardData, durationRound, currentTimerValue, null);
            statePayload.Remove(AnalyticsEvents.Parameter.Boosters);
            return statePayload;
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
            var statePayload = CreateGameplayStatePayload(locale, boardData, durationRound, currentTimerValue, boosters);
            statePayload.Remove(AnalyticsEvents.Parameter.CellsEmpty);
            statePayload[AnalyticsEvents.Parameter.EraseItem] =
                AnalyticsPayloadHelper.GetIndexedItemPayload(erasedIndex, erasedLetter);
            return statePayload;
        }

        public Dictionary<string, object> CreateSlowdownBoosterSuccessPayload(
            int slowdownDelay,
            string locale,
            int durationRound,
            float currentTimerValue,
            string[] boardData,
            IReadOnlyDictionary<BoosterType, BoosterItem> boosters)
        {
            var statePayload = CreateGameplayStatePayload(locale, boardData, durationRound, currentTimerValue, boosters);
            statePayload.Remove(AnalyticsEvents.Parameter.CellsEmpty);
            statePayload[AnalyticsEvents.Parameter.SlowdownDelay] = slowdownDelay;
            return statePayload;
        }

        public Dictionary<string, object> CreateMixerBoosterSuccessPayload(
            string locale,
            int durationRound,
            float currentTimerValue,
            string[] boardBefore,
            string[] boardAfter,
            string patternId,
            string arrangerId,
            IReadOnlyList<int> targetIndexes,
            IReadOnlyDictionary<BoosterType, BoosterItem> boosters)
        {
            var statePayload = CreateGameplayStatePayload(locale, boardAfter, durationRound, currentTimerValue, boosters);
            statePayload[AnalyticsEvents.Parameter.FieldBefore] =
                AnalyticsPayloadHelper.GetFieldPayload(boardBefore);
            statePayload[AnalyticsEvents.Parameter.FieldAfter] =
                AnalyticsPayloadHelper.GetFieldPayload(boardAfter);
            statePayload[AnalyticsEvents.Parameter.MixerPattern] = patternId;
            statePayload[AnalyticsEvents.Parameter.MixerArranger] = arrangerId;
            statePayload[AnalyticsEvents.Parameter.TargetIndexes] =
                AnalyticsPayloadHelper.GetIndexesPayload(targetIndexes);

            return statePayload;
        }

        public Dictionary<string, object> CreateMixerBoosterFailPayload(
            string locale,
            int durationRound,
            float currentTimerValue,
            string[] boardData,
            string reason,
            IReadOnlyDictionary<BoosterType, BoosterItem> boosters)
        {
            var statePayload = CreateGameplayStatePayload(locale, boardData, durationRound, currentTimerValue, boosters);
            statePayload[AnalyticsEvents.Parameter.Reason] = reason;
            return statePayload;
        }

        private Dictionary<string, object> CreateGameplayStatePayload(
            string locale,
            IReadOnlyList<string> boardData,
            int durationRound,
            float currentTimerValue,
            IReadOnlyDictionary<BoosterType, BoosterItem> boosters)
        {
            return new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Locale] = locale,
                [AnalyticsEvents.Parameter.DurationRound] = durationRound,
                [AnalyticsEvents.Parameter.DurationRoundLeft] = Mathf.Max(0, durationRound - Mathf.RoundToInt(currentTimerValue)),
                [AnalyticsEvents.Parameter.CellsEmpty] = CountEmptyCells(boardData),
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
