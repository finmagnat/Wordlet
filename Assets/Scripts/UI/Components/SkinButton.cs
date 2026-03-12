using Core.Config;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Components
{
    public class SkinButton : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Image _imagePreview;
        [SerializeField] private Image _imageCheck;
        [SerializeField] private Color activeColor;
        [SerializeField] private Color anActiveColor;
        
        public Button button;
        
        public SkinType SkinType => _skinType;
        
        public SkinType _skinType;
        
        public void SetSkinData(Sprite spritePreview, SkinType skinType)
        {
            _imagePreview.sprite = spritePreview;
            _skinType = skinType;
        }
        
        public void SetActiveStatus(bool status)
        {
            _image.color = status ? activeColor : anActiveColor;
            _imageCheck.gameObject.SetActive(status);
        }
    }
}