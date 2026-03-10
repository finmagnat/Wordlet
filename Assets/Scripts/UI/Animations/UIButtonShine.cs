using DG.Tweening;
using UnityEngine;

namespace Core.UI.Components
{
    public class UIButtonShine : MonoBehaviour
    {
        [SerializeField] private RectTransform _shine;
        [SerializeField] private float _travelDistance = 700f;
        [SerializeField] private float _duration = 0.8f;

        [Header("Delay")]
        [SerializeField] private float _minDelay = 4f;
        [SerializeField] private float _maxDelay = 7f;

        private float _startX;

        private void Awake()
        {
            _startX = _shine.anchoredPosition.x;
        }

        private void OnEnable()
        {
            PlayLoop();
        }

        private void PlayLoop()
        {
            float delay = Random.Range(_minDelay, _maxDelay);

            DOVirtual.DelayedCall(delay, () =>
            {
                _shine.anchoredPosition = new Vector2(_startX, _shine.anchoredPosition.y);

                _shine.DOAnchorPosX(_startX + _travelDistance, _duration)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(PlayLoop);
            });
        }
    }
}