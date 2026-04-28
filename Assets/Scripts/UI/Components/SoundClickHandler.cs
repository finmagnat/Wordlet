using Core.Config;
using Core.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Core.UI.Components
{
    public class SoundClickHandler : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private string _sfxKey = SoundsConfig.ButtonClick;
        
        [Inject] private AudioService _audioService;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            _audioService?.PlaySfxAsync(_sfxKey);
        }
        
        public void OnPointerClick()
        {
            _audioService?.PlaySfxAsync(_sfxKey);
        }
    }
}