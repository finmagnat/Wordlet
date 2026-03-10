using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Parallax
{
    [DisallowMultipleComponent]
    public sealed class UIFloatingSparkle : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Graphic _graphic;
        [SerializeField] private RectTransform _rectTransform;

        [Header("Alpha")]
        [SerializeField] private float _minAlpha = 0f;
        [SerializeField] private float _maxAlpha = 1f;

        [Header("Scale")]
        [SerializeField] private float _minScaleMultiplier = 0.2f;
        [SerializeField] private float _maxScaleMultiplier = 1f;

        [Header("Twinkle Timing")]
        [SerializeField] private float _minDuration = 1.8f;
        [SerializeField] private float _maxDuration = 3.4f;
        [SerializeField] private float _maxStartDelay = 2f;

        [Header("Rotation")]
        [SerializeField] private bool _useRotation = true;
        [SerializeField] private float _minRotationDuration = 6f;
        [SerializeField] private float _maxRotationDuration = 12f;
        [SerializeField] private float _rotationAngle = 20f;

        [Header("Optional Small Drift")]
        [SerializeField] private bool _useDrift = false;
        [SerializeField] private float _driftY = 4f;
        [SerializeField] private float _driftDurationMultiplier = 1.15f;

        private Vector3 _baseScale;
        private Vector2 _baseAnchoredPos;
        private float _baseRotationZ;

        private Sequence _twinkleSequence;
        private Tween _rotationTween;
        private Tween _driftTween;

        private void Awake()
        {
            if (_graphic == null)
                _graphic = GetComponent<Graphic>();

            if (_rectTransform == null)
                _rectTransform = transform as RectTransform;

            if (_rectTransform != null)
            {
                _baseScale = _rectTransform.localScale;
                _baseAnchoredPos = _rectTransform.anchoredPosition;
                _baseRotationZ = NormalizeAngle(_rectTransform.localEulerAngles.z);
            }
        }

        private void OnEnable()
        {
            if (_graphic == null || _rectTransform == null)
                return;

            PlayTwinkle();
            PlayRotation();
            PlayDrift();
        }

        private void OnDisable()
        {
            KillTweens();

            if (_graphic != null)
            {
                Color c = _graphic.color;
                c.a = _maxAlpha;
                _graphic.color = c;
            }

            if (_rectTransform != null)
            {
                _rectTransform.localScale = _baseScale;
                _rectTransform.anchoredPosition = _baseAnchoredPos;
                _rectTransform.localRotation = Quaternion.Euler(0f, 0f, _baseRotationZ);
            }
        }

        private void PlayTwinkle()
        {
            float startDelay = Random.Range(0f, _maxStartDelay);
            float duration = Random.Range(_minDuration, _maxDuration);

            float minScale = Mathf.Clamp01(_minScaleMultiplier);
            float maxScale = Mathf.Max(minScale, _maxScaleMultiplier);

            // Стартуем всегда из "полного" состояния,
            // чтобы не было скачка из случайного стартового скейла.
            Color c = _graphic.color;
            c.a = _maxAlpha;
            _graphic.color = c;

            _rectTransform.localScale = _baseScale * maxScale;

            _twinkleSequence = DOTween.Sequence()
                .SetDelay(startDelay)
                .SetUpdate(true);

            _twinkleSequence.Append(
                DOTween.To(
                        () => _graphic.color.a,
                        value =>
                        {
                            Color color = _graphic.color;
                            color.a = value;
                            _graphic.color = color;
                        },
                        _minAlpha,
                        duration * 0.5f)
                    .SetEase(Ease.InOutQuad));

            _twinkleSequence.Join(
                _rectTransform.DOScale(_baseScale * minScale, duration * 0.5f)
                    .SetEase(Ease.InOutQuad));

            _twinkleSequence.SetLoops(-1, LoopType.Yoyo);
        }

        private void PlayRotation()
        {
            if (!_useRotation || _rectTransform == null)
                return;

            float direction = Random.value > 0.5f ? 1f : -1f;
            float duration = Random.Range(_minRotationDuration, _maxRotationDuration);
            float targetAngle = _baseRotationZ + _rotationAngle * direction;

            _rotationTween = _rectTransform
                .DOLocalRotate(new Vector3(0f, 0f, targetAngle), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        private void PlayDrift()
        {
            if (!_useDrift || _rectTransform == null)
                return;

            float duration = Random.Range(_minDuration, _maxDuration) * _driftDurationMultiplier;
            float targetY = _baseAnchoredPos.y + _driftY;

            _driftTween = _rectTransform
                .DOAnchorPosY(targetY, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        private void KillTweens()
        {
            _twinkleSequence?.Kill();
            _rotationTween?.Kill();
            _driftTween?.Kill();

            _twinkleSequence = null;
            _rotationTween = null;
            _driftTween = null;
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle > 180f)
                angle -= 360f;

            while (angle < -180f)
                angle += 360f;

            return angle;
        }
    }
}