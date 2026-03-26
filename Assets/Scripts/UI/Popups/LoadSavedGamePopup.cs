using System;
using System.Collections;
using Core.Data;
using Core.Services;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Popups
{
    public class LoadSavedGamePopup : UIPopup
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _gameDataText;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _removeButton;
        [SerializeField] private RectTransform content;
        
        [Inject] private ISaveService _saveService;
        [Inject] private LocalizationService _localization;
        
        private SaveGameData _gameData;
        
        private UniTaskCompletionSource<LoadSavedGameData> _completionSource;

        private void Awake()
        {
            _startButton.onClick.AddListener(async () =>
            {
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
                await HideAsync();
                _completionSource?.TrySetResult(new LoadSavedGameData { Result = PopupResult.Close });
            });

            _removeButton.onClick.AddListener(async () =>
            {
                await HideAsync();
                _completionSource?.TrySetResult(new LoadSavedGameData { Result = PopupResult.RemoveAndExit });
            });
        }
        
        public UniTask<LoadSavedGameData> WaitForResultAsync() => _completionSource.Task;
        
        public override async UniTask ShowAsync()
        {    
            _completionSource = new UniTaskCompletionSource<LoadSavedGameData>();
         
            _gameData = await _saveService.LoadAsync();
            ChangeText();
            
            await base.ShowAsync();
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
                    _gameData.levelComplexityAI,
                    BoolToWord(_gameData.playerTurn),
                    _gameData.maxSeconds,
                    _gameData.currentSeconds,
                    _gameData.playerScore,
                    _gameData.playerPasses,
                    _gameData.opponentScore,
                    _gameData.opponentPasses,
                    _gameData.firstWord,
                    string.Join(", ", _gameData.playerWords),
                    string.Join(", ", _gameData.opponentWords));
                
                StartCoroutine(RebuildEndOfFrame());
            }
        }

        private IEnumerator RebuildEndOfFrame()
        {
            _gameDataText.ForceMeshUpdate();
            
            yield return null;                // дождаться применения размеров
            
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
        
        private string BoolToWord(bool value)
        {
            return value ? 
                _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyTextYes) : 
                _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyTextNo);
        }
    }
}