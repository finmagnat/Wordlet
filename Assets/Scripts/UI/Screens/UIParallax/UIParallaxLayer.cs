using UnityEngine;

namespace UI.Parallax
{
    [DisallowMultipleComponent]
    public sealed class UIParallaxLayer : MonoBehaviour
    {
        [SerializeField] private RectTransform _target;

        [Header("Parallax")]
        [SerializeField] private Vector2 _movement = new(30f, 24f);
        [SerializeField] private float _followSpeed = 10f;
        [SerializeField] private bool _captureInitialPositionOnEnable = false;

        [Header("Idle Float")]
        [SerializeField] private bool _useIdleFloat = true;
        [SerializeField] private Vector2 _idleAmplitude = new(6f, 4f);
        [SerializeField] private Vector2 _idleFrequency = new(0.12f, 0.08f);

        private Vector2 _initialAnchoredPosition;
        private Vector2 _currentOffset;
        private Vector2 _targetOffset;
        private bool _initialized;

        private void Awake()
        {
            EnsureTarget();
            CaptureInitialPosition();
        }

        private void OnEnable()
        {
            EnsureTarget();

            if (_captureInitialPositionOnEnable)
                CaptureInitialPosition();

            _currentOffset = Vector2.zero;
            _targetOffset = Vector2.zero;

            if (_target != null)
                _target.anchoredPosition = _initialAnchoredPosition;
        }

        public void SetOffset(Vector2 offset, bool instant = false)
        {
            _targetOffset = new Vector2(
                offset.x * _movement.x,
                offset.y * _movement.y);

            if (instant)
                _currentOffset = _targetOffset;
        }

        public void SetNormalizedOffset(Vector2 normalizedOffset)
        {
            SetOffset(normalizedOffset, false);
        }

        public void ResetLayer()
        {
            if (_target == null)
                return;

            _currentOffset = Vector2.zero;
            _targetOffset = Vector2.zero;
            _target.anchoredPosition = _initialAnchoredPosition;
        }

        public void CaptureInitialPosition()
        {
            if (_target == null)
                return;

            _initialAnchoredPosition = _target.anchoredPosition;
            _initialized = true;
        }

        private void LateUpdate()
        {
            if (_target == null || !_initialized)
                return;

            float t = 1f - Mathf.Exp(-_followSpeed * Time.unscaledDeltaTime);
            _currentOffset = Vector2.Lerp(_currentOffset, _targetOffset, t);

            Vector2 idleOffset = Vector2.zero;

            if (_useIdleFloat)
            {
                float x = Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * _idleFrequency.x) * _idleAmplitude.x;
                float y = Mathf.Cos(Time.unscaledTime * Mathf.PI * 2f * _idleFrequency.y) * _idleAmplitude.y;
                idleOffset = new Vector2(x, y);
            }

            _target.anchoredPosition = _initialAnchoredPosition + _currentOffset + idleOffset;
        }

        private void EnsureTarget()
        {
            if (_target == null)
                _target = transform as RectTransform;

            if (_target == null)
            {
                Debug.LogError($"[{nameof(UIParallaxLayer)}] RectTransform is missing on {name}", this);
                enabled = false;
            }
        }
    }
}