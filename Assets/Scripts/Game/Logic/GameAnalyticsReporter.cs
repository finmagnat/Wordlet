using System.Collections.Generic;
using Core.Config;
using Core.Services;
using Inventory;

namespace Game.Logic
{
    public sealed class GameAnalyticsReporter
    {
        private readonly AnalyticsService _analytics;
        private readonly GameAnalyticsPayloadFactory _payloadFactory;

        public GameAnalyticsReporter(
            AnalyticsService analytics,
            GameAnalyticsPayloadFactory payloadFactory)
        {
            _analytics = analytics;
            _payloadFactory = payloadFactory;
        }

        public void TrackAiGameStarted(
            ComplexityAI complexityAI,
            int durationRound,
            IReadOnlyDictionary<BoosterType, BoosterItem> boosters,
            string locale,
            string startWord)
        {
            _analytics.TrackEvent(AnalyticsEvents.GameFlow.AiGameStarted,
                _payloadFactory.CreateAiGameStartedPayload(
                    complexityAI,
                    durationRound,
                    boosters,
                    locale,
                    startWord));
        }

        public void TrackAiSavedGameStarted(
            string savedGameJson,
            IReadOnlyDictionary<BoosterType, BoosterItem> boosters)
        {
            _analytics.TrackEvent(AnalyticsEvents.GameFlow.AiSavedGameStarted,
                _payloadFactory.CreateAiSavedGameStartedPayload(savedGameJson, boosters));
        }

        public void TrackApplyWordClicked(string word)
        {
            _analytics.TrackEvent(AnalyticsEvents.GameFlow.ApplyWordClicked, new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Word] = word,
                [AnalyticsEvents.Parameter.WordLength] = word.Length
            });
        }

        public void TrackCancelClicked()
        {
            _analytics.TrackEvent(AnalyticsEvents.GameFlow.CancelClicked);
        }

        public void TrackCellUnselected(int index)
        {
            _analytics.TrackEvent(AnalyticsEvents.GameFlow.CellUnselected, new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Index] = index
            });
        }

        public void TrackCellSelected(int index)
        {
            _analytics.TrackEvent(AnalyticsEvents.GameFlow.CellSelected, new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Index] = index
            });
        }

        public void TrackLetterPutSuccess(string letter, int index)
        {
            _analytics.TrackEvent(AnalyticsEvents.GameFlow.LetterPutSuccess, new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Letter] = letter,
                [AnalyticsEvents.Parameter.Index] = index
            });
        }

        public void TrackKeyboardLetterClicked(string letter)
        {
            _analytics.TrackEvent(AnalyticsEvents.GameFlow.KeyboardLetterClicked, new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Letter] = letter
            });
        }

        public void TrackPassGameClicked(Dictionary<string, object> snapshot)
        {
            _analytics.TrackEvent(AnalyticsEvents.GameFlow.PassGameClicked, snapshot);
        }

        public void TrackReplayClicked()
        {
            _analytics.TrackEvent(AnalyticsEvents.GameFlow.ReplayClicked);
        }

        public void TrackTimeExpired(Dictionary<string, object> snapshot, bool isOwnerTurn)
        {
            snapshot[AnalyticsEvents.Parameter.WhoseMove] = isOwnerTurn
                ? AnalyticsEvents.Option.Owner
                : AnalyticsEvents.Option.Opponent;

            _analytics.TrackEvent(AnalyticsEvents.GameFlow.TimeExpired, snapshot);
        }

        public void TrackWordInfoClicked(string word)
        {
            _analytics.TrackEvent(AnalyticsEvents.GameFlow.WordInfoClicked, new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Word] = word
            });
        }

        public void TrackBoosterGameClicked(Dictionary<string, object> snapshot, BoosterType boosterType)
        {
            snapshot[AnalyticsEvents.Parameter.BoosterClicked] = boosterType.ToString();
            _analytics.TrackEvent(AnalyticsEvents.BoosterUsage.BoosterGameClicked, snapshot);
        }

        public void TrackFinishGame(Dictionary<string, object> snapshot, ResultGame resultGame, bool isSavedGame)
        {
            _analytics.TrackEvent(AnalyticsEvents.GameFlow.FinishGame,
                _payloadFactory.CreateFinishGamePayload(snapshot, resultGame, isSavedGame));
        }

        public void TrackAiMoveStart(Dictionary<string, object> snapshot)
        {
            _analytics.TrackEvent(AnalyticsEvents.GameFlow.AiMoveStart, snapshot);
        }

        public void TrackAiMoveSuccess(Dictionary<string, object> snapshot, string word)
        {
            snapshot[AnalyticsEvents.Parameter.Word] = word;
            snapshot[AnalyticsEvents.Parameter.WordLength] = word.Length;
            _analytics.TrackEvent(AnalyticsEvents.GameFlow.AiMoveSuccess, snapshot);
        }

        public void TrackAiMoveFail(Dictionary<string, object> snapshot)
        {
            _analytics.TrackEvent(AnalyticsEvents.GameFlow.AiMoveFail, snapshot);
        }
    }
}
