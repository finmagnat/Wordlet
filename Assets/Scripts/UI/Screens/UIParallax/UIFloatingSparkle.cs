using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Parallax
{
    [DisallowMultipleComponent]
    public sealed class UIFloatingSparkle : MonoBehaviour
    {
        [SerializeField] private Graphic _graphic;
        [SerializeField] private float _minAlpha = 0.25f;
        [SerializeField] private float _maxAlpha = 0.85f;
        [SerializeField] private float _duration = 1.8f;
        [SerializeField] private float _moveY = 6f;

        private Vector2 _startPos;
        private Tween _alphaTween;
        private Tween _moveTween;

        private void Awake()
        {
            if (_graphic == null)
                _graphic = GetComponent<Graphic>();

            RectTransform rt = transform as RectTransform;
            if (rt != null)
                _startPos = rt.anchoredPosition;
        }

        private void OnEnable()
        {
            if (_graphic == null)
                return;

            float randomDelay = Random.Range(0f, 1.2f);
            float randomDuration = _duration * Random.Range(0.85f, 1.2f);

            Color c = _graphic.color;
            c.a = Random.Range(_minAlpha, _maxAlpha);
            _graphic.color = c;

            _alphaTween = _graphic.DOFade(
                    Random.Range(_minAlpha, _maxAlpha),
                    randomDuration)
                .SetDelay(randomDelay)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

            RectTransform rt = transform as RectTransform;
            if (rt != null)
            {
                _moveTween = rt.DOAnchorPosY(_startPos.y + _moveY, randomDuration * 1.2f)
                    .SetDelay(randomDelay)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }

        private void OnDisable()
        {
            _alphaTween?.Kill();
            _moveTween?.Kill();
        }
    }
}