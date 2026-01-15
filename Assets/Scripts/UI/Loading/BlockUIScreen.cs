using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UI.Screens;

namespace Core.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BlockUIScreen : UIScreen
    {
        [Header("UI Elements")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _spinner;
        [SerializeField] private float _fadeDuration = 0.3f;
        [SerializeField] private float _spinSpeed = 180f;
        
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
            
            var tween = _canvasGroup.DOFade(1f, _fadeDuration);
            await UniTask.WaitUntil(() => !tween.IsPlaying());
            
            Debug.Log($"[BlockUIScreen] ShowAsync");
            // TEST-Simulate
            //await UniTask.WaitForSeconds(3.0f);
        }

        public override async UniTask HideAsync()
        {
            if (!_isVisible) return;

            _isVisible = false;
            _canvasGroup.blocksRaycasts = false;

            Debug.Log($"[BlockUIScreen] HideAsync");
            
            var tween = _canvasGroup.DOFade(0f, _fadeDuration);
            await UniTask.WaitUntil(() => !tween.IsPlaying());
        }
    }
}