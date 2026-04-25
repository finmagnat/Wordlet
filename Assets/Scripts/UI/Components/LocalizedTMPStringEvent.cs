using UnityEngine;
using TMPro;
using UnityEngine.Localization;

namespace Core.UI.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTMPStringEvent : MonoBehaviour
    {
        [SerializeField] private LocalizedString localizedString;
        
        private TMP_Text _text;
        private bool _isSubscribed;

        public LocalizedString LocalizedString => localizedString;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void SetReference(string table, string key)
        {
            if (_text == null)
                _text = GetComponent<TMP_Text>();

            if (string.IsNullOrWhiteSpace(table) || string.IsNullOrWhiteSpace(key))
            {
                _text.text = string.Empty;
                return;
            }

            localizedString.SetReference(table, key);

            if (!_isSubscribed)
                Subscribe();
        }

        private void Subscribe()
        {
            if (_isSubscribed) return;

            localizedString.StringChanged += UpdateText;

            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed) return;

            localizedString.StringChanged -= UpdateText;

            _isSubscribed = false;
        }

        private void UpdateText(string value)
        {
            if (_text != null)
                _text.text = value;
        }
    }

}
