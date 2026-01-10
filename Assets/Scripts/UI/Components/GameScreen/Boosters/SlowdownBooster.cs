using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    public class SlowdownBooster : BoosterUI
    {
        [Header("Refs")] 
        [SerializeField] private Image _overlayRadial; // CooldownOverlay (Image Filled Radial360)
        [SerializeField] private TextMeshProUGUI _counterText; // CounterText

        [Header("Options")] 
        [SerializeField] private bool _hideWhenFinished = true;
        [SerializeField] private bool _showZeroForOneSecond = false; // если хочешь, чтобы "0" повисело 1 сек

        private CancellationTokenSource _cts;

        private int _seconds;
        
        private void Awake()
        {
            SetVisible(false);
            if (_overlayRadial != null)
                _overlayRadial.fillAmount = 0f;
        }

        private void OnDisable()
        {
            CancelRunning();
        }

        public override void ActivateBooster()
        {
            if (_seconds <= 0)
            {
                Finish();
                return;
            }

            CancelRunning();
            _cts = new CancellationTokenSource();
           
            RunCountdown(_seconds, _cts.Token).Forget();
        }
        
        
        public void SetSeconds(int seconds)
        {
            if (seconds <= 0)
            {
                Finish();
                return;
            }

            _seconds = seconds;
        }

        public void Cancel()
        {
            CancelRunning();
            Finish();
        }

        private async UniTaskVoid RunCountdown(int totalSeconds, CancellationToken token)
        {
            IsActive = true;
            SetVisible(true);

            // Старт
            SetState(totalSeconds, totalSeconds);

            int remaining = totalSeconds;

            // Логика: показываем N..1, потом 0 (опционально)
            while (remaining > 0)
            {
                // Важно: fillAmount = доля оставшегося времени
                SetState(remaining, totalSeconds);

                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                remaining--;
            }

            // remaining == 0
            SetState(0, totalSeconds);

            if (_showZeroForOneSecond)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }

            Finish();
        }

        private void SetState(int remainingSeconds, int totalSeconds)
        {
            if (_counterText != null)
                _counterText.text = remainingSeconds.ToString();

            if (_overlayRadial != null)
            {
                float t = totalSeconds <= 0 ? 0f : (float)remainingSeconds / totalSeconds;
                _overlayRadial.fillAmount = Mathf.Clamp01(t); // 1 -> 0
            }
        }

        private void Finish()
        {
            if (_overlayRadial != null)
                _overlayRadial.fillAmount = 0f;

            if (_hideWhenFinished)
                SetVisible(false);
            else
                SetVisible(true); // если хочешь оставить "0" + пустой круг
            
            IsActive = false;
        }

        private void SetVisible(bool visible)
        {
            if (_overlayRadial != null)
                _overlayRadial.gameObject.SetActive(visible);

            if (_counterText != null)
                _counterText.gameObject.SetActive(visible);
        }

        private void CancelRunning()
        {
            if (_cts == null) return;
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }
}