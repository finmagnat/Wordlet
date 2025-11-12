using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Components
{
    public class LanguageButton : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Color activeColor;
        [SerializeField] private Color anActiveColor;
        [SerializeField] private Color activeColorText;
        [SerializeField] private Color anActiveColorText;

        public Button button;
        public string language;
        private bool isActive;
        
        public void SetText(string s)
        {
            text.text = s;
        }

        public void SetActiveStatus(bool status)
        {
            _image.color = status ? activeColor : anActiveColor;
            text.color = status ? activeColorText : anActiveColorText;
        }
    }
}