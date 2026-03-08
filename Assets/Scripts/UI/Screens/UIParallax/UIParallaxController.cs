using System;
using Core.Config;
using Core.Events;
using UnityEngine;

namespace UI.Parallax
{
    public sealed class UIParallaxController : MonoBehaviour
    {
        [SerializeField] private UIParallaxLayer[] _layers;

        [Header("Mode")]
        [SerializeField] private UIParallaxMode _mode = UIParallaxMode.TouchAndAuto;

        [Header("Axis")]
        [SerializeField] private bool _invertX;
        [SerializeField] private bool _invertY;

        [Header("Auto Motion")]
        [SerializeField] private Vector2 _autoMotionAmplitude = new(0.18f, 0.12f);
        [SerializeField] private Vector2 _autoMotionFrequency = new(0.05f, 0.08f);

        [Header("Touch")]
        [SerializeField] private float _touchStrength = 1f;

        [Header("Gyro")]
        [SerializeField] private float _gyroStrengthX = 0.35f;
        [SerializeField] private float _gyroStrengthY = 0.20f;
        [SerializeField] private float _gyroSmoothing = 4f;

        private Vector2 _gyroSmoothed;

        public UIParallaxMode Mode
        {
            get => _mode;
            set => _mode = value;
        }

        private void Awake()
        {
            var gyroEnabled = PlayerPrefs.GetInt(PlayerPrefsKey.GyroKey, 1);
            SetMode(gyroEnabled);
            
            EventBus.Subscribe<GyroEnableEvent>(OnGyroEnableChanged);
        }
       
        private void OnDestroy()
        {
            EventBus.Unsubscribe<GyroEnableEvent>(OnGyroEnableChanged);
        }

        private void OnEnable()
        {
            ApplyOffset(Vector2.zero);
        }

        private void Update()
        {
            Vector2 normalizedOffset = GetNormalizedOffset();
            ApplyOffset(normalizedOffset);
        }

        private Vector2 GetNormalizedOffset()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return GetEditorOffset();
#else
            return GetMobileOffset();
#endif
        }

        private Vector2 GetEditorOffset()
        {
            switch (_mode)
            {
                case UIParallaxMode.Off:
                    return Vector2.zero;

                case UIParallaxMode.Auto:
                    return GetAutoMotionOffset();

                case UIParallaxMode.Touch:
                case UIParallaxMode.Gyro:
                case UIParallaxMode.TouchAndAuto:
                case UIParallaxMode.GyroAndAuto:
                    // В редакторе и на ПК используем мышь как удобный surrogate input
                    Vector2 mouse = GetMouseOffset();
                    if (_mode == UIParallaxMode.TouchAndAuto || _mode == UIParallaxMode.GyroAndAuto)
                    {
                        Vector2 auto = GetAutoMotionOffset();
                        return CombineOffsets(mouse, auto);
                    }
                    return mouse;

                default:
                    return Vector2.zero;
            }
        }

        private Vector2 GetMobileOffset()
        {
            switch (_mode)
            {
                case UIParallaxMode.Off:
                    return Vector2.zero;

                case UIParallaxMode.Auto:
                    return GetAutoMotionOffset();

                case UIParallaxMode.Touch:
                    return GetTouchOffsetOrZero();

                case UIParallaxMode.Gyro:
                    return GetGyroOffset();

                case UIParallaxMode.TouchAndAuto:
                {
                    Vector2 touch = GetTouchOffsetOrZero();
                    Vector2 auto = GetAutoMotionOffset();
                    return CombineOffsets(touch, auto);
                }

                case UIParallaxMode.GyroAndAuto:
                {
                    Vector2 gyro = GetGyroOffset();
                    Vector2 auto = GetAutoMotionOffset();
                    return CombineOffsets(gyro, auto);
                }

                default:
                    return Vector2.zero;
            }
        }

        private Vector2 GetMouseOffset()
        {
            Vector2 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);

            if (screenCenter.x <= 0f || screenCenter.y <= 0f)
                return Vector2.zero;

            Vector2 delta = (Vector2)Input.mousePosition - screenCenter;

            Vector2 normalized = new(
                Mathf.Clamp(delta.x / screenCenter.x, -1f, 1f),
                Mathf.Clamp(delta.y / screenCenter.y, -1f, 1f));

            return ApplyInvert(normalized);
        }

        private Vector2 GetTouchOffsetOrZero()
        {
            if (Input.touchCount <= 0)
                return Vector2.zero;

            Touch touch = Input.GetTouch(0);

            Vector2 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);

            if (screenCenter.x <= 0f || screenCenter.y <= 0f)
                return Vector2.zero;

            Vector2 delta = touch.position - screenCenter;

            Vector2 normalized = new(
                Mathf.Clamp(delta.x / screenCenter.x, -1f, 1f),
                Mathf.Clamp(delta.y / screenCenter.y, -1f, 1f));

            normalized *= _touchStrength;

            normalized.x = Mathf.Clamp(normalized.x, -1f, 1f);
            normalized.y = Mathf.Clamp(normalized.y, -1f, 1f);

            return ApplyInvert(normalized);
        }

        private Vector2 GetGyroOffset()
        {
            Vector3 acc = Input.acceleration;

            Vector2 raw = new(
                Mathf.Clamp(acc.x * _gyroStrengthX, -1f, 1f),
                Mathf.Clamp(acc.y * _gyroStrengthY, -1f, 1f));

            _gyroSmoothed = Vector2.Lerp(
                _gyroSmoothed,
                raw,
                Time.unscaledDeltaTime * Mathf.Max(0.01f, _gyroSmoothing));

            return ApplyInvert(_gyroSmoothed);
        }

        private Vector2 GetAutoMotionOffset()
        {
            Vector2 autoOffset = new(
                Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * _autoMotionFrequency.x) * _autoMotionAmplitude.x,
                Mathf.Cos(Time.unscaledTime * Mathf.PI * 2f * _autoMotionFrequency.y) * _autoMotionAmplitude.y);

            return ApplyInvert(autoOffset);
        }

        private Vector2 CombineOffsets(Vector2 primary, Vector2 secondary)
        {
            Vector2 combined = primary + secondary;
            combined.x = Mathf.Clamp(combined.x, -1f, 1f);
            combined.y = Mathf.Clamp(combined.y, -1f, 1f);
            return combined;
        }

        private Vector2 ApplyInvert(Vector2 value)
        {
            if (_invertX)
                value.x *= -1f;

            if (_invertY)
                value.y *= -1f;

            return value;
        }

        private void ApplyOffset(Vector2 normalizedOffset)
        {
            if (_layers == null)
                return;

            for (int i = 0; i < _layers.Length; i++)
            {
                if (_layers[i] != null)
                    _layers[i].SetNormalizedOffset(normalizedOffset);
            }
        }

        public void SetMode(int mode)
        {
            _mode = (UIParallaxMode)mode;
        }

        public void SetMode(UIParallaxMode mode)
        {
            _mode = mode;
        }
        
        private void OnGyroEnableChanged(GyroEnableEvent enableEvent)
        {
            _mode = enableEvent.ParallaxMode;
        }

    }
}