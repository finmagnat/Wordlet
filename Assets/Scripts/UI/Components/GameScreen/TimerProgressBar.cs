using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    public class TimerProgressBar : MonoBehaviour
    {
        private const float DtDelay = 1.0f; // Интервал 1 секунда
        
        [SerializeField] private Slider _slider; // Компонент Slider

        private float _dtTimer = 0.0f; // Инкрементный счетчик времени (дельтатайм) (при достижении DtDelay увеличивается _secondsCounter)
        private bool _bRun = false; // Старт/Пауза таймера

        private void Awake()
        {
            _slider.value = 0;
            //SetTargetValue(10, true); //test
        }

        // Update is called once per frame
        private void Update()
        {
            if (_bRun)
            {
                _dtTimer += Time.deltaTime;
                if (_dtTimer >= DtDelay)
                {
                    _dtTimer = 0;
                    if (++_slider.value >= _slider.maxValue)
                    {
                        StopTimer();
                        //EventAggregator.TimeExpired.Publish(); // TODO: 
                    }
                }
            }
        }
        
        public void SetTargetValue(float value, bool autostartTimer = false)
        {
            if (value > 0)
            {
                _slider.maxValue = value;

                if (autostartTimer)
                    StartTimer();
            }
        }
                
        public void StartTimer() => _bRun = true;

        public void StopTimer() => _bRun = false;

        public void ResetTimer()
        {
            _bRun = false;
            _dtTimer = 0;
            _slider.value = 0;
        }
        
    }
}