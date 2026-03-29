using Core.Generated;
using Core.Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Core.UI.Components
{
    [RequireComponent(typeof(Toggle))]
    public class SoundToggle : MonoBehaviour
    {
        [SerializeField] private AssetKey _sfxAssetKey = AssetKey.sfx_button_click;
        
        [Inject] private AudioService _audioService;
        
        private Toggle _toggle;
        
        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener((bool value) =>
            {
                _audioService?.PlaySfxAsync(_sfxAssetKey.ToString());
            });
        }
    }
}