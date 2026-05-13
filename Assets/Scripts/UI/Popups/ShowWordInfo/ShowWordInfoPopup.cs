using System.Collections.Generic;
using Core.Data;
using Core.Services;
using Core.Services.ReportWord;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Popups
{
    public class ShowWordInfoPopup : UIPopup<ShowWordInfoWindowEventData>
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _wordText;
        [SerializeField] private TextMeshProUGUI _infoText;
        [SerializeField] private TMP_Dropdown _reasonDropdown;
        [SerializeField] private TextMeshProUGUI _cooldownText;
        [SerializeField] private Button _sendButton;

        [Inject] private LocalizationService _localization;
        [Inject] private AnalyticsService _analytics;
        
        private ShowWordInfoWindowEventData _eventData;
        private UniTaskCompletionSource<PopupExitData> _completionSource;
        
        protected void Start()
        {
            _cooldownText.text = "";
            _cooldownText.gameObject.SetActive(false);
            
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(OnCloseClicked);
            }
            
            _sendButton.onClick.AddListener(async () =>
            {                
                await HideAsync();
                Close();
                _completionSource?.TrySetResult(new PopupExitData { Result = PopupResult.SaveAndExit });
            });
            
            _reasonDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }
        
        private void OnDropdownChanged(int index)
        {
            Debug.Log($"Выбран пункт: {index}");
            _sendButton.interactable = GetSelectedReason() != ReportReason.None;
        }

        private void OnDestroy()
        {
            _reasonDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        }
        
        public override UniTask PrepareAsync(ShowWordInfoWindowEventData data)
        {
            _eventData = data;
            _wordText.text = data.word;
            _infoText.text = string.IsNullOrWhiteSpace(data.definition)
                ? _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyWordDefinitionPending)
                : data.definition;
            
            var options = new List<TMP_Dropdown.OptionData>(ReportReasonExtensions.Reasons.Count);
            foreach (ReportReason reason in ReportReasonExtensions.Reasons)
                options.Add(new TMP_Dropdown.OptionData(_localization.Get(LocalizationConst.TableUI, ReportReasonExtensions.ToLocaleKey(reason))));
            
            _reasonDropdown.ClearOptions();
            _reasonDropdown.AddOptions(options);
            _reasonDropdown.value = 0;
            _reasonDropdown.RefreshShownValue();
            
            _sendButton.interactable = false;

            return UniTask.CompletedTask;
        }
        
        public override async UniTask ShowAsync()
        {
            _completionSource = new UniTaskCompletionSource<PopupExitData>();
            await base.ShowAsync();
            _analytics.TrackEvent(AnalyticsEvents.Navigation.WordInfoPopupShown, new Dictionary<string, object>
            {
                [AnalyticsEvents.Parameter.Locale] = _localization.CurrentLocale.Identifier.Code,
                [AnalyticsEvents.Parameter.Word] = _eventData?.word ?? string.Empty
            });
        }
        
        public UniTask<PopupExitData> WaitForResultAsync() => _completionSource.Task;
        
        public void SetSubmitState(bool canSubmit, string message)
        {
            _reasonDropdown.interactable = canSubmit;
            
            if (_cooldownText == null)
                return;
            
            bool showMessage = !string.IsNullOrWhiteSpace(message);
            _cooldownText.gameObject.SetActive(showMessage);

            if (showMessage)
                _cooldownText.text = message;
        }
        
        public ReportReason GetSelectedReason()
        {
            int index = _reasonDropdown.value;

            if (index < 0 || index >= ReportReasonExtensions.Reasons.Count)
                return ReportReason.None;

            return ReportReasonExtensions.Reasons[index];
        }
        
        protected virtual void OnCloseClicked()
        {
            _analytics.TrackEvent(AnalyticsEvents.Navigation.CloseWordInfoClicked);
            HandleButtonClick(PopupResult.Close).Forget();
        }

        protected virtual async UniTaskVoid HandleButtonClick(PopupResult result)
        {
            await HideAsync();
            Close();
            _completionSource?.TrySetResult(new PopupExitData { Result = result });
        }
        
        private void Close()
        {
            _wordText.text = "";
            _infoText.text = "";
            _cooldownText.text = "";
            _cooldownText.gameObject.SetActive(false);
        }
    }
}
