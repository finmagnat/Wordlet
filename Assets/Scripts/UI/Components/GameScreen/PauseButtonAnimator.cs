using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    public class PauseButtonAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform buttonRect;
        [SerializeField] private Graphic optionalGlow; // например Image-подсветка (можно null)
        [SerializeField] private bool isLevitation; // Включение анимации левитации

        private Vector3 _baseScale;
        private Vector2 _basePos;
        private float _baseGlowAlpha;

        private Sequence _seq;

        private void Awake()
        {
            if (!buttonRect) buttonRect = (RectTransform)transform;

            _baseScale = buttonRect.localScale;
            _basePos = buttonRect.anchoredPosition;

            if (optionalGlow)
                _baseGlowAlpha = optionalGlow.color.a;
        }

        public void SetPaused(bool paused)
        {
            if (paused) Play();
            else Stop();
        }

        private void Play()
        {
            Stop(); // на всякий случай

            _seq = DOTween.Sequence()
                .SetUpdate(true) // важно для паузы при timeScale=0
                .SetAutoKill(false);

            // 1) дыхание
            _seq.Append(buttonRect.DOScale(_baseScale * 1.05f, 0.9f).SetEase(Ease.InOutSine));
            _seq.Append(buttonRect.DOScale(_baseScale, 0.9f).SetEase(Ease.InOutSine));
            
            // 2) левитация параллельно (Join)
            if (isLevitation)
                _seq.Join(
                    buttonRect.DOAnchorPosY(_basePos.y + 6f, 1.2f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo));

            // 3) подсветка (опционально)
            if (optionalGlow)
            {
                var c = optionalGlow.color;
                _seq.Join(
                    DOTween.To(() => optionalGlow.color.a, a =>
                        {
                            var cc = optionalGlow.color;
                            cc.a = a;
                            optionalGlow.color = cc;
                        }, _baseGlowAlpha * 0.25f, 0.9f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetUpdate(true)
                );
            }

            _seq.SetLoops(-1);
            _seq.Play();
        }

        private void Stop()
        {
            if (_seq != null)
            {
                _seq.Kill();
                _seq = null;
            }

            // вернуть базу
            buttonRect.localScale = _baseScale;
            
            if (isLevitation)
                buttonRect.anchoredPosition = _basePos;

            if (optionalGlow)
            {
                var c = optionalGlow.color;
                c.a = _baseGlowAlpha;
                optionalGlow.color = c;
            }
        }

        private void OnDestroy()
        {
            _seq?.Kill();
        }
    }
}