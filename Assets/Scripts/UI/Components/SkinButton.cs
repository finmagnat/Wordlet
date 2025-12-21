using Core.Config;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Components
{
    public class SkinButton : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Image _imagePreview;
        [SerializeField] private Color activeColor;
        [SerializeField] private Color anActiveColor;
        [SerializeField] private Color activeColorText;
        [SerializeField] private Color anActiveColorText;
        
        public Button button;
        
        public SkinType SkinType => _skinType;
        
        private bool _isActive;
        public SkinType _skinType;
        
        public void SetSkinData(Sprite sprite, SkinType skinType)
        {
            _imagePreview.sprite = sprite;
            _skinType = skinType;
        }
        
        public void SetActiveStatus(bool status)
        {
            _isActive = status;
            _image.color = status ? activeColor : anActiveColor;
        }
    }
}