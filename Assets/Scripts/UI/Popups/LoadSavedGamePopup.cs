using System;
using System.Collections.Generic;
using Core.Config;
using Core.Data;
using Core.Services;
using Core.UI;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Popups
{
    public class LoadSavedGamePopup : UIPopup<NoPayload>
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _gameDataText;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _removeButton;
        [SerializeField] private RectTransform content;

        [Inject] private ISaveService _saveService;
        [Inject] private ConfigService _configService;
        [Inject] private LocalizationService _localization;
        [Inject] private AnalyticsService _analytics;
        [Inject] private GameAnalyticsPayloadFactory _analyticsPayloadFactory;
        [Inject] private IInventoryService _inventory;

        private SaveGameData _gameData;

        private UniTaskCompletionSource<LoadSavedGameData> _completionSource;

        protected override void Awake()
        {
            base.Awake();

            _startButton.onClick.AddListener(async () =>
            {
                _analytics.TrackEvent(
                    AnalyticsEvents.Navigation.PlaySavedGameClicked,
                    GetAnalyticsParams());

                await HideAsync();

                var data = new LoadSavedGameData
                {
                    Result = PopupResult.Play,
                    GameData = _gameData
                };
                _completionSource?.TrySetResult(data);
            });

            _closeButton.onClick.AddListener(async () =>
            {
                _analytics.TrackEvent(AnalyticsEvents.Navigation.CloseSavedGameClicked);
                await HideAsync();
                _completionSource?.TrySetResult(new LoadSavedGameData { Result = PopupResult.Close });
            });

            _removeButton.onClick.AddListener(async () =>
            {
                _analytics.TrackEvent(AnalyticsEvents.Navigation.RemoveSavedGameClicked);
                await HideAsync();
                _completionSource?.TrySetResult(new LoadSavedGameData { Result = PopupResult.RemoveAndExit });
            });
        }

        public UniTask<LoadSavedGameData> WaitForResultAsync() => _completionSource.Task;

        public override async UniTask PrepareAsync(NoPayload payload)
        {
            _gameData = await _saveService.LoadAsync();
            ChangeText();
        }

        public override async UniTask ShowAsync()
        {
            _completionSource = new UniTaskCompletionSource<LoadSavedGameData>();
            await base.ShowAsync();
            _analytics.TrackEvent(AnalyticsEvents.Navigation.SavedGamePopupShown);
        }

        protected override async UniTask BeforeShowAnimationAsync()
        {
            RefreshLayout();
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            RefreshLayout();
        }

        private void ChangeText()
        {
            if (_gameData != null)
            {
                _gameDataText.text = _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyPopupSavedGameText,
                    _gameData.version,
                    TicksToLocalString(_gameData.savedAtUtcTicks),
                    _gameData.localeCode,
                    _gameData.mode,
                    _localization.Get(LocalizationConst.TableUI, ComplexityToLocaleKey(_gameData.levelComplexityAI)),
                    BoolToWord(_gameData.playerTurn),
                    GetFormatMMSS(_gameData.maxSeconds),
                    GetFormatMMSS(_gameData.maxSeconds - _gameData.currentSeconds),
                    _gameData.playerScore,
                    _gameData.playerPasses,
                    _gameData.opponentScore,
                    _gameData.opponentPasses,
                    _gameData.firstWord,
                    string.Join(", ", _gameData.playerWords),
                    string.Join(", ", _gameData.opponentWords));
            }
        }

        private void RefreshLayout()
        {
            _gameDataText.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            Canvas.ForceUpdateCanvases();
        }

        private string TicksToLocalString(long ticks)
        {
            return new DateTime(ticks, DateTimeKind.Utc)
                .ToLocalTime()
                .ToString("dd.MM.yyyy HH:mm:ss");
        }

        private string GetFormatMMSS(int seconds)
        {
            if (seconds < 0)
                seconds = 0;

            int m = seconds / 60;
            int s = seconds % 60;
            return $"{m:0}:{s:00}";
        }

        private string ComplexityToLocaleKey(int complexityAI)
        {
            return (ComplexityAI)complexityAI switch
            {
                ComplexityAI.EASY => "POPUP_LABEL_DIFFICULTY_EASY",
                ComplexityAI.NORMAL => "POPUP_LABEL_DIFFICULTY_NORMAL",
                ComplexityAI.HARD => "POPUP_LABEL_DIFFICULTY_HARD",
                ComplexityAI.MASTER => "POPUP_LABEL_DIFFICULTY_MASTER",
                _ => "none"
            };
        }

        private string BoolToWord(bool value)
        {
            return value ?
                _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyTextYes) :
                _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyTextNo);
        }

        private Dictionary<string, object> GetAnalyticsParams()
        {
            if (_gameData == null)
                return new Dictionary<string, object>();

            uint maxPasses = _configService.Game
                .GetComplexityAIItem((ComplexityAI)_gameData.levelComplexityAI)
                .MaxPasses;

            return _analyticsPayloadFactory.CreateGameSnapshotPayload(_gameData, maxPasses, _inventory.Boosters);
        }
    }
}
