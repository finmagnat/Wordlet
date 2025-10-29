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

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rect = GetComponent<RectTransform>();
            _canvasGroup.alpha = 0f;
            _rect.localScale = Vector3.one * 0.95f;
            gameObject.SetActive(false);
        }

        public virtual async UniTask ShowAsync()
        {
            OnShowStarted?.Invoke();
            gameObject.SetActive(true);

            Sequence seq = DOTween.Sequence();
            seq.Join(_canvasGroup.DOFade(1f, _fadeDuration));
            seq.Join(_rect.DOScale(1f + _scalePunch, _scaleDuration * 0.5f).SetEase(Ease.OutBack));
            seq.Append(_rect.DOScale(1f, _scaleDuration * 0.5f));

            await seq.AsyncWaitForCompletion();

            OnShowCompleted?.Invoke();
        }

        public virtual async UniTask HideAsync()
        {
            OnHideStarted?.Invoke();

            Sequence seq = DOTween.Sequence();
            seq.Join(_canvasGroup.DOFade(0f, _fadeDuration));
            seq.Join(_rect.DOScale(1f - _scalePunch, _scaleDuration));

            await seq.AsyncWaitForCompletion();

            gameObject.SetActive(false);
            OnHideCompleted?.Invoke();
        }
    }
}