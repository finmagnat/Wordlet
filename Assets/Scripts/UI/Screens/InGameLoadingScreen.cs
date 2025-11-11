using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace Core.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class InGameLoadingScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _spinner;
        [SerializeField] private TMP_Text _loadingText;
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

        public async UniTask ShowAsync()
        {
            if (_isVisible) return;

            _isVisible = true;
            _canvasGroup.blocksRaycasts = true;
            _loadingText.text = "Загрузка...";

            var tween = _canvasGroup.DOFade(1f, _fadeDuration);
            await UniTask.WaitUntil(() => !tween.IsPlaying());
        }

        public async UniTask HideAsync()
        {
            if (!_isVisible) return;

            _isVisible = false;
            _canvasGroup.blocksRaycasts = false;

            var tween = _canvasGroup.DOFade(0f, _fadeDuration);
            await UniTask.WaitUntil(() => !tween.IsPlaying());
        }
    }
}