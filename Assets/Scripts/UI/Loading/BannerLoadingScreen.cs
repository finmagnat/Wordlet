using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UI.Screens;

namespace Core.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BannerLoadingScreen : UIScreen
    {
        [Header("UI Elements")]
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected float _fadeDuration = 0.3f;
        [SerializeField] protected GameObject[] _banners;
#if UNITY_EDITOR
        [Header("DEBUG")]
        [SerializeField] protected bool _isDebug;
        [SerializeField] protected int _indexAlways = 0;
#endif
        
        protected bool _isVisible;

        protected virtual void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
            _isVisible = false;
        }

        public override async UniTask ShowAsync()
        {
            if (_isVisible) return;
            
            int index = Random.Range(0, _banners.Length);
#if UNITY_EDITOR
            if(_isDebug) index = _indexAlways;
#endif
            for (int i = 0; i < _banners.Length; ++i)
            {
                if (i == index)
                    _banners[i].SetActive(true);
                else
                    _banners[i].SetActive(false);
            }

            _isVisible = true;
            _canvasGroup.blocksRaycasts = true;
            
            var tween = _canvasGroup.DOFade(1f, _fadeDuration);
            await UniTask.WaitUntil(() => !tween.IsPlaying());
            
            Debug.Log($"[InGameLoadingScreen] ShowAsync: {index}");
            // TEST-Simulate
            //await UniTask.WaitForSeconds(3.0f);
        }

        public override async UniTask HideAsync()
        {
            if (!_isVisible) return;

            _isVisible = false;
            _canvasGroup.blocksRaycasts = false;

            Debug.Log($"[InGameLoadingScreen] HideAsync");
            
            var tween = _canvasGroup.DOFade(0f, _fadeDuration);
            await UniTask.WaitUntil(() => !tween.IsPlaying());
        }
    }
}