using Core.Generated;
using Core.Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Core.UI.Components
{
    [RequireComponent(typeof(Button))]
    public class SoundButton : MonoBehaviour
    {
        [SerializeField] private AssetKey _sfxAssetKey = AssetKey.sfx_button_click;
        
        [Inject] private AudioService _audioService;
        
        private Button _button;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() =>
            {
                _audioService?.PlaySfxAsync(_sfxAssetKey.ToString());
            });
        }
    }
}