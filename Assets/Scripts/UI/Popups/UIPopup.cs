using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace UI.Popups
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIPopup : MonoBehaviour
    {
        protected CanvasGroup _canvasGroup;
        protected RectTransform _rect;

        public event Action OnShowStarted;
        public event Action OnShowCompleted;
        public event Action OnHideStarted;
        public event Action OnHideCompleted;

        [Header("Animation Settings")]
        [SerializeField] private float _fadeDuration = 0.25f;
        [SerializeField] private float _scaleDuration = 0.3f;
        [SerializeField] private float _scalePunch = 0.05f;

        private bool _initialized;

        protected virtual void Awake()
        {
            Initialize();
            gameObject.SetActive(false);
        }

        private void Initialize()
        {
            if (_initialized) return;

            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            if (_rect == null)
                _rect = GetComponent<RectTransform>();

            _canvasGroup.alpha = 0f;
            _rect.localScale = Vector3.one * 0.95f;

            _initialized = true;
        }

        public virtual async UniTask ShowAsync()
        {
            Initialize();

            OnShowStarted?.Invoke();
            gameObject.SetActive(true);

            var seq = DOTween.Sequence();
            seq.Join(_canvasGroup.DOFade(1f, _fadeDuration));
            seq.Join(_rect.DOScale(1f + _scalePunch, _scaleDuration * 0.5f).SetEase(Ease.OutBack));
            seq.Append(_rect.DOScale(1f, _scaleDuration * 0.5f));

            await seq.AsyncWaitForCompletion();

            OnShowCompleted?.Invoke();
        }

        public virtual async UniTask HideAsync()
        {
            Initialize();

            OnHideStarted?.Invoke();

            var seq = DOTween.Sequence();
            seq.Join(_canvasGroup.DOFade(0f, _fadeDuration));
            seq.Join(_rect.DOScale(1f - _scalePunch, _scaleDuration));

            await seq.AsyncWaitForCompletion();

            gameObject.SetActive(false);
            OnHideCompleted?.Invoke();
        }
    }
}
