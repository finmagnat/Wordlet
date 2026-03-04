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
        
        public SkinType _skinType;
        
        public void SetSkinData(Color colorPreviewTile, SkinType skinType)
        {
            _imagePreview.color = colorPreviewTile;
            _skinType = skinType;
        }
        
        public void SetActiveStatus(bool status)
        {
            _image.color = status ? activeColor : anActiveColor;
        }
    }
}