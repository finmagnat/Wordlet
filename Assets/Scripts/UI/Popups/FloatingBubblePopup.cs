using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace UI.Popups
{
    public class FloatingBubblePopup : UIPopup
    {
        [Header("References")]
        [SerializeField] private RectTransform _animatedRoot;
        [SerializeField] private CanvasGroup _canvasGroupPopup;

        [Header("Animation")]
        [SerializeField] private float _showDuration = 1.2f;
        [SerializeField] private float _moveUpDistance = 80f;
        [SerializeField] private Ease _moveEase = Ease.OutCubic;
        [SerializeField] private Ease _fadeEase = Ease.OutCubic;

        [Header("Timing")]
        [SerializeField] private float _visibleDelayBeforeFloat = 0f;

        private Vector2 _startAnchoredPosition;
        private Tween _moveTween;
        private Tween _fadeTween;
        private Sequence _sequence;

        private void Awake()
        {
            if (_animatedRoot == null)
                _animatedRoot = transform as RectTransform;

            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            _startAnchoredPosition = _animatedRoot.anchoredPosition;
        }

        public override async UniTask ShowAsync()
        {
            ResetVisualState();

            await base.ShowAsync();
            PlayFloatingAnimation();
        }

        public override async UniTask HideAsync()
        {
            KillTweens();
            await base.HideAsync();
        }

        private void PlayFloatingAnimation()
        {
            KillTweens();

            _sequence = DOTween.Sequence().SetUpdate(true);

            if (_visibleDelayBeforeFloat > 0f)
                _sequence.AppendInterval(_visibleDelayBeforeFloat);

            _moveTween = _animatedRoot.DOAnchorPosY(
                _startAnchoredPosition.y + _moveUpDistance,
                _showDuration);

            _fadeTween = _canvasGroup.DOFade(0f, _showDuration);

            _moveTween.SetEase(_moveEase);
            _fadeTween.SetEase(_fadeEase);

            _sequence.Join(_moveTween);
            _sequence.Join(_fadeTween);

            _sequence.OnComplete(OnFloatingAnimationComplete);
        }

        private void ResetVisualState()
        {
            KillTweens();

            _animatedRoot.anchoredPosition = _startAnchoredPosition;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        private void KillTweens()
        {
            _moveTween?.Kill();
            _moveTween = null;

            _fadeTween?.Kill();
            _fadeTween = null;

            _sequence?.Kill();
            _sequence = null;
        }

        protected virtual void OnFloatingAnimationComplete()
        {
            IsVisible = false;
        }

        private void OnDisable()
        {
            KillTweens();
        }

        private void OnDestroy()
        {
            KillTweens();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_animatedRoot == null)
                _animatedRoot = transform as RectTransform;

            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }
#endif
    }
}
