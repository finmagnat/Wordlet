using Cysharp.Threading.Tasks;
using TMPro;
using UI.Screens;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UI
{
    public class LoadingScreen : UIScreen
    {
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _percentText;

        public void SetProgress01(float value01)
        {
            if (_progressBar) _progressBar.value = Mathf.Clamp01(value01);
            if (_percentText) _percentText.text = Mathf.RoundToInt(value01 * 100f) + "%";
        }

        public override UniTask ShowAsync()
        {
            gameObject.SetActive(true);
            SetProgress01(0f);
            return UniTask.CompletedTask;
        }
    }
}