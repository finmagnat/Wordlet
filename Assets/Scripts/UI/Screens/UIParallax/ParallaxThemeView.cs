using Core.Data;
using Core.Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Parallax
{
    public class ParallaxThemeView : MonoBehaviour
    {
        [Header("Layers")]
        [SerializeField] private Image _sky;
        [SerializeField] private Image _cloudsFar;
        [SerializeField] private Image _cloudsMid;
        [SerializeField] private Image _cloudsNear;
        [SerializeField] private Image _atmosphericLight;
        //[SerializeField] private Image _glow; // TODO: пока нет этого слоя

        [Inject] protected SkinsService _skinsService;

        private void Start()
        {
            UpdateTheme(_skinsService.SkinCurrent);
            _skinsService.OnSkinChanged += UpdateTheme;
        }

        private void OnDestroy()
        {
            _skinsService.OnSkinChanged -= UpdateTheme;
        }
        
        private void UpdateTheme(SkinData skin)
        {
            var theme = skin.MainScreenTheme;
            _sky.color = theme.SkyColor;
            _cloudsFar.color = theme.CloudsFarColor;
            _cloudsMid.color = theme.CloudsMidColor;
            _cloudsNear.color = theme.CloudsNearColor;
            _atmosphericLight.color = theme.AtmosphericLightColor;
            //_glow.color = theme.GlowColor;

            //Debug.LogWarning($"Theme not found: {themeId}");
        }

    }
}