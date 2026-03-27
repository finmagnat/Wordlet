using Core.Events;
using UnityEngine;

namespace Core.UI.Components
{
    [RequireComponent(typeof(CanvasGroup))]
    public class AdsBackgroundOverlay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _overlay;

        private void Awake()
        {
            EventBus.Subscribe<ShowAdsEvent>(OnShowAdsEvent);
            Hide();
        }
        
        private void OnDestroy()
        {
            EventBus.Unsubscribe<ShowAdsEvent>(OnShowAdsEvent);
        }

        private void OnShowAdsEvent(ShowAdsEvent adsEvent)
        {
            if(adsEvent.IsShow)
                Show();
            else
                Hide();
        }

        public void Show()
        {
            _overlay.alpha = 1f;
            _overlay.blocksRaycasts = true;
        }

        public void Hide()
        {
            _overlay.alpha = 0f;
            _overlay.blocksRaycasts = false;
        }
    }
}