using UnityEngine;

namespace UI.Parallax
{
    [DisallowMultipleComponent]
    public sealed class UIParallaxLayer : MonoBehaviour
    {
        [SerializeField] private RectTransform _target;

        [Header("Parallax")]
        [SerializeField] private Vector2 _movement = new(20f, 12f);
        [SerializeField] private float _smoothTime = 0.2f;

        [Header("Idle Float")]
        [SerializeField] private bool _useIdleFloat = true;
        [SerializeField] private Vector2 _idleAmplitude = new(6f, 4f);
        [SerializeField] private Vector2 _idleFrequency = new(0.12f, 0.08f);

        private Vector2 _initialAnchoredPosition;
        private Vector2 _velocity;
        private Vector2 _parallaxOffset;

        private void Awake()
        {
            if (_target == null)
                _target = transform as RectTransform;

            if (_target == null)
            {
                Debug.LogError($"[{nameof(UIParallaxLayer)}] RectTransform is missing on {name}", this);
                enabled = false;
                return;
            }

            _initialAnchoredPosition = _target.anchoredPosition;
        }

        public void SetNormalizedOffset(Vector2 normalizedOffset)
        {
            _parallaxOffset = new Vector2(
                normalizedOffset.x * _movement.x,
                normalizedOffset.y * _movement.y);
        }

        public void ResetLayer()
        {
            if (_target == null)
                return;

            _target.anchoredPosition = _initialAnchoredPosition;
            _velocity = Vector2.zero;
            _parallaxOffset = Vector2.zero;
        }

        private void LateUpdate()
        {
            if (_target == null)
                return;

            Vector2 idleOffset = Vector2.zero;

            if (_useIdleFloat)
            {
                float x = Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * _idleFrequency.x) * _idleAmplitude.x;
                float y = Mathf.Cos(Time.unscaledTime * Mathf.PI * 2f * _idleFrequency.y) * _idleAmplitude.y;
                idleOffset = new Vector2(x, y);
            }

            Vector2 targetPosition = _initialAnchoredPosition + _parallaxOffset + idleOffset;

            _target.anchoredPosition = Vector2.SmoothDamp(
                _target.anchoredPosition,
                targetPosition,
                ref _velocity,
                _smoothTime);
        }
    }
}