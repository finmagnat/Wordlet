using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UI.Loading;
using UI.Screens;
using Core.Services;
using Zenject;

namespace Core.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BannerLoadingScreen : UIScreen
    {
        public BannerItemUI CurrentBanner { get; private set; }

        [Inject] private AnalyticsService _analytics;

        [Header("UI Elements")] [SerializeField]
        protected CanvasGroup _canvasGroup;

        [SerializeField] protected float _fadeDuration = 0.3f;
        [SerializeField] protected BannerItemUI[] _banners;
#if UNITY_EDITOR
        [Header("DEBUG")] [SerializeField] protected bool _isDebug;
        [SerializeField] protected int _indexAlways = 0;
#endif

        protected bool _isVisible;

        protected virtual void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
            _isVisible = false;
        }

        public override async UniTask ShowAsync()
        {
            if (_isVisible) return;

            UpdateBanners();

            if (CurrentBanner != null)
            {
                _analytics.TrackEvent(
                    AnalyticsEvents.Navigation.BannerLoadingShown,
                    new System.Collections.Generic.Dictionary<string, object>
                    {
                        [AnalyticsEvents.Parameter.Banner] = CurrentBanner.BannerType.ToString()
                    });
            }

            _isVisible = true;
            _canvasGroup.blocksRaycasts = true;

            var tween = _canvasGroup.DOFade(1f, _fadeDuration);
            await UniTask.WaitUntil(() => !tween.IsPlaying());

            // TEST-Simulate
            //await UniTask.WaitForSeconds(3.0f);
        }

        public override async UniTask HideAsync()
        {
            if (!_isVisible) return;

            _isVisible = false;
            _canvasGroup.blocksRaycasts = false;

            Debug.Log("[BannerLoadingScreen] HideAsync");

            var tween = _canvasGroup.DOFade(0f, _fadeDuration);
            await UniTask.WaitUntil(() => !tween.IsPlaying());
        }

        protected void UpdateBanners()
        {
            if (_banners == null || _banners.Length == 0)
            {
                CurrentBanner = null;
                Debug.LogError($"[BannerLoadingScreen] No banners configured on {name}", this);
                return;
            }

            int index = Random.Range(0, _banners.Length);
#if UNITY_EDITOR
            if (_isDebug) index = _indexAlways;
#endif
            CurrentBanner = _banners[index];

            for (int i = 0; i < _banners.Length; ++i)
            {
                if (_banners[i] == null)
                    continue;

                _banners[i].gameObject.SetActive(i == index);
            }

            if (CurrentBanner == null)
            {
                Debug.LogError($"[BannerLoadingScreen] Banner at index {index} is null on {name}", this);
                return;
            }

            Debug.Log($"[BannerLoadingScreen] [UpdateBanners] CurrentBanner: {CurrentBanner.BannerType}");
        }
    }
}
