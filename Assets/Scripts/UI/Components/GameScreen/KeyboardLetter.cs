using Core.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    public class KeyboardLetter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _letterText;
        [SerializeField] private Image _mainBackground;
        
        public void SetLetter(string letter) => _letterText.text = letter;
        
        public void SetSkin(Sprite sprite) => _mainBackground.sprite = sprite;
        
        public void OnClick()
        {
            EventBus.Raise(new KeyboardLetterSelectEvent{ letter = _letterText.text });
        }
    }
}
