using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    [RequireComponent(typeof(Button))]
    public class ButtonAnimator : MonoBehaviour
    {
        private Vector3 _initialScale;
        private Tween _scaleTween;

        private void Awake()
        {
            _initialScale = transform.localScale;

            Button button = GetComponent<Button>();
            button.onClick.AddListener(PlayBreath);
        }

        private void PlayBreath()
        {
            // если уже дышит — убиваем прошлый твин
            _scaleTween?.Kill();

            transform.localScale = _initialScale;

            _scaleTween = transform
                .DOScale(_initialScale * 1.05f, 0.2f)
                .SetEase(Ease.OutQuad)
                .SetLoops(2, LoopType.Yoyo);
        }
    }
}