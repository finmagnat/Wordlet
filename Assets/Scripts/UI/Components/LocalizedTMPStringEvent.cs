using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Core.UI.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTMPStringEvent : MonoBehaviour
    {
        [SerializeField] private LocalizedString localizedString;

        private TMP_Text _text;
        private bool _isSubscribed;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            Refresh();
        }

        private void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_isSubscribed) return;

            localizedString.StringChanged += UpdateText;
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed) return;

            localizedString.StringChanged -= UpdateText;
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;

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