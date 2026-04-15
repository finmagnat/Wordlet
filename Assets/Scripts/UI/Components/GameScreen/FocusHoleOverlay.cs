using TMPro;
using UI.Popups;
using UnityEngine;

namespace UI.Components
{
    public class FocusHoleOverlay : UIPopup
    {
        [Header("Root")]
        [SerializeField] private RectTransform _overlayRoot;

        [Header("Blocks")]
        [SerializeField] private RectTransform _topBlock;
        [SerializeField] private RectTransform _bottomBlock;
        [SerializeField] private RectTransform _leftBlock;
        [SerializeField] private RectTransform _rightBlock;

        [Header("Optional")]
        [SerializeField] private TMP_Text _hintText;
        [SerializeField] private RectTransform _hintAnchor;

        [Header("Settings")]
        [SerializeField] private Vector2 _padding = new Vector2(16f, 16f);
        [SerializeField] private bool _updateContinuously = true;

        private RectTransform _target;
        private Canvas _rootCanvas;
        private Camera _uiCamera;
        private bool _isShown;

        private readonly Vector3[] _targetWorldCorners = new Vector3[4];
        private readonly Vector3[] _overlayWorldCorners = new Vector3[4];

        public bool IsShown => _isShown;
        public RectTransform Target => _target;

        private void Awake()
        {
            if (_overlayRoot == null)
                _overlayRoot = transform as RectTransform;

            _rootCanvas = GetComponentInParent<Canvas>();

            if (_rootCanvas != null)
            {
                if (_rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    _uiCamera = null;
                else
                    _uiCamera = _rootCanvas.worldCamera;
            }

            HideImmediate();
        }

        private void LateUpdate()
        {
            if (!_isShown || !_updateContinuously || _target == null)
                return;

            Refresh();
        }

        public void Show(RectTransform target, string hint = null)
        {
            _target = target;
            _isShown = true;
            gameObject.SetActive(true);

            if (_hintText != null)
                _hintText.text = hint ?? string.Empty;

            Refresh();
        }

        public void Hide()
        {
            _target = null;
            _isShown = false;
            gameObject.SetActive(false);
        }

        public void HideImmediate()
        {
            _target = null;
            _isShown = false;
            gameObject.SetActive(false);
        }

        public void Refresh()
        {
            if (_target == null || _overlayRoot == null)
                return;

            Rect targetRectInOverlay = GetTargetRectInOverlaySpace(_target, _overlayRoot);

            targetRectInOverlay.xMin -= _padding.x;
            targetRectInOverlay.xMax += _padding.x;
            targetRectInOverlay.yMin -= _padding.y;
            targetRectInOverlay.yMax += _padding.y;

            Rect overlayRect = _overlayRoot.rect;

            float left = Mathf.Clamp(targetRectInOverlay.xMin, overlayRect.xMin, overlayRect.xMax);
            float right = Mathf.Clamp(targetRectInOverlay.xMax, overlayRect.xMin, overlayRect.xMax);
            float bottom = Mathf.Clamp(targetRectInOverlay.yMin, overlayRect.yMin, overlayRect.yMax);
            float top = Mathf.Clamp(targetRectInOverlay.yMax, overlayRect.yMin, overlayRect.yMax);

            SetRectStretch(_topBlock, overlayRect.xMin, top, overlayRect.xMax, overlayRect.yMax);
            SetRectStretch(_bottomBlock, overlayRect.xMin, overlayRect.yMin, overlayRect.xMax, bottom);
            SetRectStretch(_leftBlock, overlayRect.xMin, bottom, left, top);
            SetRectStretch(_rightBlock, right, bottom, overlayRect.xMax, top);

            UpdateHintPosition(left, right, top);
        }

        private void UpdateHintPosition(float left, float right, float top)
        {
            if (_hintAnchor == null)
                return;

            float centerX = (left + right) * 0.5f;
            float hintY = top + 40f;

            _hintAnchor.anchorMin = new Vector2(0.5f, 0.5f);
            _hintAnchor.anchorMax = new Vector2(0.5f, 0.5f);
            _hintAnchor.pivot = new Vector2(0.5f, 0f);
            _hintAnchor.anchoredPosition = new Vector2(centerX, hintY);
        }

        private Rect GetTargetRectInOverlaySpace(RectTransform target, RectTransform overlay)
        {
            target.GetWorldCorners(_targetWorldCorners);
            overlay.GetWorldCorners(_overlayWorldCorners);

            Vector2 min = RectTransformUtility.WorldToScreenPoint(_uiCamera, _targetWorldCorners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(_uiCamera, _targetWorldCorners[2]);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay, min, _uiCamera, out Vector2 localMin);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay, max, _uiCamera, out Vector2 localMax);

            return Rect.MinMaxRect(localMin.x, localMin.y, localMax.x, localMax.y);
        }

        private void SetRectStretch(RectTransform rect, float xMin, float yMin, float xMax, float yMax)
        {
            if (rect == null)
                return;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            float width = Mathf.Max(0f, xMax - xMin);
            float height = Mathf.Max(0f, yMax - yMin);
            float centerX = xMin + width * 0.5f;
            float centerY = yMin + height * 0.5f;

            rect.anchoredPosition = new Vector2(centerX, centerY);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
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