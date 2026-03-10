using DG.Tweening;
using UnityEngine;

namespace Core.UI.Components
{
    public class UIButtonIdlePulse : MonoBehaviour
    {
        [SerializeField] private RectTransform _target;

        [Header("Pulse")]
        [SerializeField] private float _scale = 1.04f;
        [SerializeField] private float _duration = 0.8f;

        [Header("Delay")]
        [SerializeField] private float _minDelay = 2f;
        [SerializeField] private float _maxDelay = 4f;

        private Vector3 _baseScale;
        private Tween _pulseTween;

        private void Awake()
        {
            if (_target == null)
                _target = transform as RectTransform;

            _baseScale = _target.localScale;
        }

        private void OnEnable()
        {
            PlayLoop();
        }

        private void OnDisable()
        {
            _pulseTween?.Kill();
        }

        private void PlayLoop()
        {
            float delay = Random.Range(_minDelay, _maxDelay);

            _pulseTween = DOVirtual.DelayedCall(delay, () =>
            {
                _target
                    .DOScale(_baseScale * _scale, _duration * 0.5f)
                    .SetEase(Ease.OutSine)
                    .OnComplete(() =>
                    {
                        _target
                            .DOScale(_baseScale, _duration * 0.5f)
                            .SetEase(Ease.InSine)
                            .OnComplete(PlayLoop);
                    });
            });
        }
    }
}