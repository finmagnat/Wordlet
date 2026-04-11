using Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UI
{
    public class StartLoadingScreen : BannerLoadingScreen
    {
        [Header("Progress Elements")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _loadingText;
        [SerializeField] private TextMeshProUGUI _percentText;

        protected override void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            _canvasGroup.alpha = 1;
            _canvasGroup.blocksRaycasts = true;
            _isVisible = true;
            _loadingText.text = "";
        }
        
        public void SetProgress(float value01)
        {
            if (_progressBar) _progressBar.value = Mathf.Clamp01(value01);
            if (_percentText) _percentText.text = Mathf.RoundToInt(value01 * 100f) + "%";
        }
    }
}