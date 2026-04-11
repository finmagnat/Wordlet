using Core.Services;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using Zenject;

namespace Core.UI.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTMPStringEvent : MonoBehaviour
    {
        [SerializeField] private LocalizedString localizedString;

        [Inject] private LocalizationService _localization;
        
        private TMP_Text _text;
        private bool _isSubscribed;

        private void Start()
        {
            _text = GetComponent<TMP_Text>();
            if (LocalizationService.IsInitialized)
            {
                Initialise();
            }
            else
            {
                LocalizationService.OnInitialized += Initialise;
            }
        }

        private void Initialise()
        {
            LocalizationService.OnInitialized -= Initialise;
            Subscribe();
            Refresh();
        }
        
        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_isSubscribed) return;

            localizedString.StringChanged += UpdateText;
            _localization.OnLocaleChanged += OnLocaleChanged;

            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed) return;

            localizedString.StringChanged -= UpdateText;
            _localization.OnLocaleChanged -= OnLocaleChanged;

            _isSubscribed = false;
        }

        private void OnLocaleChanged(Locale _)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_text == null) _text = GetComponent<TMP_Text>();

            localizedString.GetLocalizedStringAsync().Completed += (handle) =>
            {
                if (handle.IsDone)
                    _text.text = handle.Result;
            };
        }

        private void UpdateText(string value)
        {
            if (_text != null)
                _text.text = value;
        }
    }

}