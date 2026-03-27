using Core.Events;
using UnityEngine;

public class AdsBackgroundOverlay : MonoBehaviour
{
    [SerializeField] private CanvasGroup _overlay;

    private int _locks;

    private void Awake()
    {
        if (_overlay == null)
            _overlay = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<AdsOverlayAcquireEvent>(OnAcquire);
        EventBus.Subscribe<AdsOverlayReleaseEvent>(OnRelease);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<AdsOverlayAcquireEvent>(OnAcquire);
        EventBus.Unsubscribe<AdsOverlayReleaseEvent>(OnRelease);
    }

    private void OnAcquire(AdsOverlayAcquireEvent _)
    {
        _locks++;
        UpdateState();
    }

    private void OnRelease(AdsOverlayReleaseEvent _)
    {
        _locks = Mathf.Max(0, _locks - 1);
        UpdateState();
    }

    private void UpdateState()
    {
        bool visible = _locks > 0;

        _overlay.alpha = visible ? 1f : 0f;
        _overlay.blocksRaycasts = visible;
    }
}