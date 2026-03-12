using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Parallax
{
    public sealed class UISkyBreathing : MonoBehaviour
    {
        [SerializeField] private Graphic _graphic;

        [SerializeField] private float _minAlpha = 0.22f;
        [SerializeField] private float _maxAlpha = 0.32f;

        [SerializeField] private float _duration = 6f;

        private Tween _tween;

        private void Awake()
        {
            if (_graphic == null)
                _graphic = GetComponent<Graphic>();
        }

        private void OnEnable()
        {
            if (_graphic == null)
                return;

            Color c = _graphic.color;
            c.a = _minAlpha;
            _graphic.color = c;

            _tween = _graphic.DOFade(_maxAlpha, _duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void OnDisable()
        {
            _tween?.Kill();
        }
    }
}