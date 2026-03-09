using Core.Config;
using Core.Events;
using UnityEngine;

namespace UI.Parallax
{
    public sealed class UIParallaxController : MonoBehaviour
    {
        [SerializeField] private UIParallaxLayer[] _layers;

        [Header("Mode")]
        [SerializeField] private UIParallaxMode _mode = UIParallaxMode.TouchAndGyro;

        [Header("Axis")]
        [SerializeField] private bool _invertX;
        [SerializeField] private bool _invertY;

        [Header("Auto Motion")]
        [SerializeField] private Vector2 _autoMotionAmplitude = new(0.35f, 0.20f);
        [SerializeField] private Vector2 _autoMotionFrequency = new(0.05f, 0.08f);

        [Header("Touch")]
        [SerializeField] private float _touchStrength = 1f;

        [Header("Gyro")]
        [SerializeField] private int _gyroStrengthX = 8;
        [SerializeField] private int _gyroStrengthY = 8;
        [SerializeField] private int _gyroSmoothing = 4;
        [SerializeField] private float _gyroMultiplier = 3f;

        public int GyroStrengthX
        {
            get => _gyroStrengthX;
            set => _gyroStrengthX = Mathf.Max(0, value);
        }

        public int GyroStrengthY
        {
            get => _gyroStrengthY;
            set => _gyroStrengthY = Mathf.Max(0, value);
        }

        public int GyroSmoothing
        {
            get => _gyroSmoothing;
            set => _gyroSmoothing = Mathf.Max(1, value);
        }

        public Vector2 AutoMotionAmplitude
        {
            get => _autoMotionAmplitude;
            set => _autoMotionAmplitude = value;
        }

        public Vector2 AutoMotionFrequency
        {
            get => _autoMotionFrequency;
            set => _autoMotionFrequency = value;
        }

        public bool InvertX
        {
            get => _invertX;
            set => _invertX = value;
        }

        public bool InvertY
        {
            get => _invertY;
            set => _invertY = value;
        }

        public Vector2 DebugRawAcceleration => Input.acceleration;
        public Vector2 DebugGyroSmoothed => _gyroSmoothed;

        private Vector2 _gyroSmoothed;

        public UIParallaxMode Mode
        {
            get => _mode;
            set => _mode = value;
        }

        private void Awake()
        {
            int gyroEnabled = PlayerPrefs.GetInt(PlayerPrefsKey.GyroKey, 1);
            SetMode(gyroEnabled != 0 ? UIParallaxMode.TouchAndGyro : UIParallaxMode.Touch);

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
            Vector2 result = GetMouseOffset();
            result += GetAutoMotionOffset();
            return Vector2.ClampMagnitude(result, 1f);
        }

        private Vector2 GetMobileOffset()
        {
            Vector2 result = GetTouchOffsetOrZero();

            if (_mode == UIParallaxMode.TouchAndGyro)
                result += GetGyroOffset();

            result += GetAutoMotionOffset();

            return Vector2.ClampMagnitude(result, 1f);
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

            normalized *= _touchStrength;
            return ApplyInvert(Vector2.ClampMagnitude(normalized, 1f));
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

            return ApplyInvert(Vector2.ClampMagnitude(normalized, 1f));
        }

        private Vector2 GetGyroOffset()
        {
            Vector3 acc = Input.acceleration;

            Vector2 raw = new(
                acc.x * _gyroStrengthX,
                acc.y * _gyroStrengthY);

            _gyroSmoothed = Vector2.Lerp(
                _gyroSmoothed,
                raw,
                Time.unscaledDeltaTime * Mathf.Max(0.01f, _gyroSmoothing));

            Vector2 result = ApplyInvert(_gyroSmoothed);
            result *= _gyroMultiplier;

            return Vector2.ClampMagnitude(result, 1f);
        }

        private Vector2 GetAutoMotionOffset()
        {
            Vector2 autoOffset = new(
                Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * _autoMotionFrequency.x) * _autoMotionAmplitude.x,
                Mathf.Cos(Time.unscaledTime * Mathf.PI * 2f * _autoMotionFrequency.y) * _autoMotionAmplitude.y);

            return ApplyInvert(autoOffset);
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
            _mode = mode != 0 ? UIParallaxMode.TouchAndGyro : UIParallaxMode.Touch;
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
