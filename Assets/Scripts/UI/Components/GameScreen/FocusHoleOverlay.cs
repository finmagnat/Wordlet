using Cysharp.Threading.Tasks;
using DG.Tweening;
using UI.Popups;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    public class FocusHoleOverlay : UIPopup
    {
        [Header("Root")]
        [SerializeField] private RectTransform _overlayRoot;

        [Header("Target")]
        [SerializeField] private RectTransform _target;

        [Header("Blocks")]
        [SerializeField] private RectTransform _topBlock;
        [SerializeField] private RectTransform _bottomBlock;
        [SerializeField] private RectTransform _leftBlock;
        [SerializeField] private RectTransform _rightBlock;

        [Header("Padding")]
        [SerializeField] private float _paddingLeft = 16f;
        [SerializeField] private float _paddingRight = 16f;
        [SerializeField] private float _paddingTop = 16f;
        [SerializeField] private float _paddingBottom = 16f;

        [Header("Focus Frame")]
        [SerializeField] private RectTransform _focusFrame;
        [SerializeField] private Image _focusFrameImage;

        [Header("Frame Margins")]
        [SerializeField] private float _frameOffsetLeft = 0f;
        [SerializeField] private float _frameOffsetRight = 0f;
        [SerializeField] private float _frameOffsetTop = 0f;
        [SerializeField] private float _frameOffsetBottom = 0f;

        [Header("Behavior")]
        [SerializeField] private bool _updateContinuously = true;

        [Header("Frame Animation")]
        [SerializeField] private bool _animateFrame = true;
        [SerializeField] private float _frameShowPunchScale = 0.04f;
        [SerializeField] private float _frameShowPunchDuration = 0.25f;
        [SerializeField] private int _frameShowPunchVibrato = 8;

        [Header("Frame Alpha Pulse")]
        [SerializeField] private bool _animateFrameAlpha = true;
        [SerializeField] [Range(0f, 1f)] private float _frameAlphaMin = 0.82f;
        [SerializeField] [Range(0f, 1f)] private float _frameAlphaMax = 1f;
        [SerializeField] private float _frameAlphaDuration = 0.9f;
        [SerializeField] private Ease _frameAlphaEase = Ease.InOutSine;

        private Canvas _rootCanvas;
        private Camera _uiCamera;
        private bool _isShown;

        private Tween _framePunchTween;
        private Tween _frameLoopTween;

        private readonly Vector3[] _targetWorldCorners = new Vector3[4];

        public bool IsShown => _isShown;
        public RectTransform Target => _target;

        private void Awake()
        {
            if (_overlayRoot == null)
                _overlayRoot = transform as RectTransform;

            _rootCanvas = GetComponentInParent<Canvas>();

            if (_rootCanvas != null)
            {
                _uiCamera = _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : _rootCanvas.worldCamera;
            }

            HideImmediate();
        }

        private void LateUpdate()
        {
            if (!_isShown || !_updateContinuously || _target == null)
                return;

            Refresh();
        }

        public override async UniTask ShowAsync()
        {
            Show();
            await base.ShowAsync();
        }

        public override async UniTask HideAsync()
        {
            StopFrameAnimation();

            await base.HideAsync();
            Hide();
        }

        public void Show()
        {
            if (_target == null)
            {
                Debug.LogError("[FocusHoleOverlay] Show called but Target is NULL.");
                return;
            }

            _isShown = true;
            gameObject.SetActive(true);

            Canvas.ForceUpdateCanvases();
            Refresh();
            PlayFrameAnimation();
        }

        public void Hide()
        {
            StopFrameAnimation();

            _isShown = false;
            gameObject.SetActive(false);
        }

        public void HideImmediate()
        {
            StopFrameAnimation();

            _isShown = false;
            gameObject.SetActive(false);
        }

        public void Refresh()
        {
            if (_target == null || _overlayRoot == null)
                return;

            Rect targetRectInOverlay = GetTargetRectInOverlaySpace(_target, _overlayRoot);

            targetRectInOverlay.xMin -= _paddingLeft;
            targetRectInOverlay.xMax += _paddingRight;
            targetRectInOverlay.yMin -= _paddingBottom;
            targetRectInOverlay.yMax += _paddingTop;

            Rect overlayRect = _overlayRoot.rect;

            float left = Mathf.Clamp(targetRectInOverlay.xMin, overlayRect.xMin, overlayRect.xMax);
            float right = Mathf.Clamp(targetRectInOverlay.xMax, overlayRect.xMin, overlayRect.xMax);
            float bottom = Mathf.Clamp(targetRectInOverlay.yMin, overlayRect.yMin, overlayRect.yMax);
            float top = Mathf.Clamp(targetRectInOverlay.yMax, overlayRect.yMin, overlayRect.yMax);

            SetRect(_topBlock, overlayRect.xMin, top, overlayRect.xMax, overlayRect.yMax);
            SetRect(_bottomBlock, overlayRect.xMin, overlayRect.yMin, overlayRect.xMax, bottom);
            SetRect(_leftBlock, overlayRect.xMin, bottom, left, top);
            SetRect(_rightBlock, right, bottom, overlayRect.xMax, top);

            float frameLeft = left - _frameOffsetLeft;
            float frameRight = right + _frameOffsetRight;
            float frameBottom = bottom - _frameOffsetBottom;
            float frameTop = top + _frameOffsetTop;

            SetRect(_focusFrame, frameLeft, frameBottom, frameRight, frameTop);
        }

        private Rect GetTargetRectInOverlaySpace(RectTransform target, RectTransform overlay)
        {
            target.GetWorldCorners(_targetWorldCorners);

            Vector2 screenBottomLeft = RectTransformUtility.WorldToScreenPoint(_uiCamera, _targetWorldCorners[0]);
            Vector2 screenTopRight = RectTransformUtility.WorldToScreenPoint(_uiCamera, _targetWorldCorners[2]);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlay,
                screenBottomLeft,
                _uiCamera,
                out Vector2 localBottomLeft);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlay,
                screenTopRight,
                _uiCamera,
                out Vector2 localTopRight);

            return Rect.MinMaxRect(
                localBottomLeft.x,
                localBottomLeft.y,
                localTopRight.x,
                localTopRight.y);
        }

        private void SetRect(RectTransform rect, float xMin, float yMin, float xMax, float yMax)
        {
            if (rect == null)
                return;

            float width = Mathf.Max(0f, xMax - xMin);
            float height = Mathf.Max(0f, yMax - yMin);
            float centerX = xMin + width * 0.5f;
            float centerY = yMin + height * 0.5f;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(centerX, centerY);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private void PlayFrameAnimation()
        {
            if (!_animateFrame || _focusFrame == null)
                return;

            StopFrameAnimation();

            _focusFrame.localScale = Vector3.one;

            if (_focusFrameImage != null)
            {
                _focusFrameImage.DOKill();

                Color color = _focusFrameImage.color;
                color.a = _frameAlphaMax;
                _focusFrameImage.color = color;
            }

            _framePunchTween = _focusFrame
                .DOPunchScale(Vector3.one * _frameShowPunchScale, _frameShowPunchDuration, _frameShowPunchVibrato, 0.8f)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (_focusFrame == null || !_isShown)
                        return;

                    _focusFrame.localScale = Vector3.one;

                    if (_animateFrameAlpha && _focusFrameImage != null)
                    {
                        _frameLoopTween = _focusFrameImage
                            .DOFade(_frameAlphaMin, _frameAlphaDuration)
                            .SetEase(_frameAlphaEase)
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetUpdate(true);
                    }
                });
        }

        private void StopFrameAnimation()
        {
            _framePunchTween?.Kill();
            _framePunchTween = null;

            _frameLoopTween?.Kill();
            _frameLoopTween = null;

            if (_focusFrame != null)
                _focusFrame.localScale = Vector3.one;

            if (_focusFrameImage != null)
            {
                _focusFrameImage.DOKill();

                Color color = _focusFrameImage.color;
                color.a = _frameAlphaMax;
                _focusFrameImage.color = color;
            }
        }

        private void OnDisable()
        {
            StopFrameAnimation();
        }

        private void OnDestroy()
        {
            StopFrameAnimation();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_overlayRoot == null)
                _overlayRoot = transform as RectTransform;
        }
#endif
    }
}