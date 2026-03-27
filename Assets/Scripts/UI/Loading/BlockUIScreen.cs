using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UI.Screens;

namespace Core.UI
{
    public enum BlockUIScreenMode
    {
        Default,
        NoSpinner,
    }

    [RequireComponent(typeof(CanvasGroup))]
    public class BlockUIScreen : UIScreen<BlockUIScreenMode>
    {
        [Header("UI Elements")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _spinner;
        [SerializeField] private float _fadeDuration = 0.3f;
        [SerializeField] private float _spinSpeed = 180f;

        private bool _isVisible;
        private bool _isVisibleSpinner;
        private BlockUIScreenMode _mode = BlockUIScreenMode.Default;

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;

            _spinner.gameObject.SetActive(true);

            _isVisible = false;
            _isVisibleSpinner = false;
        }

        private void Update()
        {
            if (_isVisibleSpinner)
                _spinner.transform.Rotate(Vector3.forward, -_spinSpeed * Time.deltaTime);
        }

        public override UniTask PrepareAsync(BlockUIScreenMode mode)
        {
            _mode = mode;

            _isVisibleSpinner = false;
            _spinner.gameObject.SetActive(_mode == BlockUIScreenMode.Default);

            return UniTask.CompletedTask;
        }

        public override async UniTask ShowAsync()
        {
            if (_isVisible)
                return;

            _isVisible = true;
            _isVisibleSpinner = _mode == BlockUIScreenMode.Default;
            _canvasGroup.blocksRaycasts = true;

            var tween = _canvasGroup.DOFade(1f, _fadeDuration);
            await tween.AsyncWaitForCompletion();

            Debug.Log("[BlockUIScreen] ShowAsync");
        }

        public override async UniTask HideAsync()
        {
            if (!_isVisible)
                return;

            _isVisible = false;
            _isVisibleSpinner = false;
            _canvasGroup.blocksRaycasts = false;

            Debug.Log("[BlockUIScreen] HideAsync");

            var tween = _canvasGroup.DOFade(0f, _fadeDuration);
            await tween.AsyncWaitForCompletion();
        }
    }
}