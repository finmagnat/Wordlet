using Core.Generated;
using Core.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Core.UI.Components
{
    public class SoundClickHandler : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private AssetKey _sfxAssetKey = AssetKey.sfx_button_click;
        
        [Inject] private AudioService _audioService;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            _audioService?.PlaySfxAsync(_sfxAssetKey.ToString());
        }
    }
}