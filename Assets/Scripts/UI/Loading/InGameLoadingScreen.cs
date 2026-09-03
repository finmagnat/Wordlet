using Core.Services;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UI.Screens;
using Zenject;

namespace Core.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class InGameLoadingScreen : UIScreen
    {
        [Header("UI Elements")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _spinner;
        [SerializeField] private TMP_Text _loadingText;
        [SerializeField] private float _fadeDuration = 0.3f;
        [SerializeField] private float _spinSpeed = 180f;
        [SerializeField] private float _delayBeforeHide = 1.0f;

        [Inject] private LocalizationService _localization;
        
        private bool _isVisible;

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
            _isVisible = false;
        }

        private void Update()
        {
            if (_isVisible && _spinner != null)
                _spinner.transform.Rotate(Vector3.forward, -_spinSpeed * Time.deltaTime);
        }

        public override async UniTask ShowAsync()
        {
            if (_isVisible) return;

            _isVisible = true;
            _canvasGroup.blocksRaycasts = true;
            _loadingText.text = _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyLabelLoading);
            
            var tween = _canvasGroup.DOFade(1f, _fadeDuration);
            await UniTask.WaitUntil(() => !tween.IsActive() || !tween.IsPlaying());
            
            Debug.Log($"[InGameLoadingScreen] ShowAsync");
            // TEST-Simulate
            //await UniTask.WaitForSeconds(3.0f);
        }

        public override async UniTask HideAsync()
        {
            if (!_isVisible) return;

            await UniTask.WaitForSeconds(_delayBeforeHide);
            
            _isVisible = false;
            _canvasGroup.blocksRaycasts = false;

            Debug.Log($"[InGameLoadingScreen] HideAsync");
            
            var tween = _canvasGroup.DOFade(0f, _fadeDuration);
            await UniTask.WaitUntil(() => !tween.IsActive() || !tween.IsPlaying());
        }
    }
}
