using Core.Config;
using Core.Events;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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

        [Header("Final Offset Clamp")]
        [SerializeField] private Vector2 _maxOffset = new(2.5f, 2.5f);

        [Header("Auto Motion")]
        [SerializeField] private Vector2 _autoMotionAmplitude = new(0.35f, 0.20f);
        [SerializeField] private Vector2 _autoMotionFrequency = new(0.05f, 0.08f);

        [Header("Touch")]
        [SerializeField] private float _touchStrengthX = 1.5f;
        [SerializeField] private float _touchStrengthY = 2.25f;

        [Header("Editor Preview")]
        [SerializeField] private bool _simulateGyroInEditor = true;
        [SerializeField] private float _editorMouseStrengthX = 1.5f;
        [SerializeField] private float _editorMouseStrengthY = 2.25f;
        [SerializeField] private float _editorGyroStrengthX = 0.75f;
        [SerializeField] private float _editorGyroStrengthY = 1.10f;

        [Header("Gyro")]
        [SerializeField] private float _gyroStrengthX = 8f;
        [SerializeField] private float _gyroStrengthY = 10f;
        [SerializeField] private float _gyroSmoothing = 6f;
        [SerializeField] private float _gyroMultiplier = 1.5f;

        private Vector2 _gyroSmoothed;

        public UIParallaxMode Mode
        {
            get => _mode;
            set => _mode = value;
        }

        public float TouchStrengthX
        {
            get => _touchStrengthX;
            set => _touchStrengthX = Mathf.Max(0f, value);
        }

        public float TouchStrengthY
        {
            get => _touchStrengthY;
            set => _touchStrengthY = Mathf.Max(0f, value);
        }

        public bool SimulateGyroInEditor
        {
            get => _simulateGyroInEditor;
            set => _simulateGyroInEditor = value;
        }

        public float EditorMouseStrengthX
        {
            get => _editorMouseStrengthX;
            set => _editorMouseStrengthX = Mathf.Max(0f, value);
        }

        public float EditorMouseStrengthY
        {
            get => _editorMouseStrengthY;
            set => _editorMouseStrengthY = Mathf.Max(0f, value);
        }

        public float EditorGyroStrengthX
        {
            get => _editorGyroStrengthX;
            set => _editorGyroStrengthX = Mathf.Max(0f, value);
        }

        public float EditorGyroStrengthY
        {
            get => _editorGyroStrengthY;
            set => _editorGyroStrengthY = Mathf.Max(0f, value);
        }

        public float GyroStrengthX
        {
            get => _gyroStrengthX;
            set => _gyroStrengthX = Mathf.Max(0f, value);
        }

        public float GyroStrengthY
        {
            get => _gyroStrengthY;
            set => _gyroStrengthY = Mathf.Max(0f, value);
        }

        public float GyroSmoothing
        {
            get => _gyroSmoothing;
            set => _gyroSmoothing = Mathf.Max(0.01f, value);
        }

        public float GyroMultiplier
        {
            get => _gyroMultiplier;
            set => _gyroMultiplier = Mathf.Max(0f, value);
        }

        public Vector2 MaxOffset
        {
            get => _maxOffset;
            set => _maxOffset = new Vector2(Mathf.Max(0f, value.x), Mathf.Max(0f, value.y));
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

        public Vector2 DebugRawAcceleration => ReadAcceleration();
        public Vector2 DebugGyroSmoothed => _gyroSmoothed;
        public Vector2 DebugFinalOffset { get; private set; }

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
            _gyroSmoothed = Vector2.zero;
            DebugFinalOffset = Vector2.zero;
            ApplyOffset(Vector2.zero, true);
        }

        private void Update()
        {
            Vector2 offset = GetFinalOffset();
            DebugFinalOffset = offset;
            ApplyOffset(offset, false);
        }

        private Vector2 GetFinalOffset()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return GetEditorOffset();
#else
            return GetMobileOffset();
#endif
        }

        private Vector2 GetEditorOffset()
        {
            Vector2 result = GetMouseOffset(_editorMouseStrengthX, _editorMouseStrengthY);

            if (_simulateGyroInEditor && _mode == UIParallaxMode.TouchAndGyro)
            {
                Vector2 simulatedGyro = GetMouseOffset(_editorGyroStrengthX, _editorGyroStrengthY);
                result += simulatedGyro;
            }

            result += GetAutoMotionOffset();
            return ClampPerAxis(ApplyInvert(result));
        }

        private Vector2 GetMobileOffset()
        {
            Vector2 result = GetTouchOffsetOrZero();

            if (_mode == UIParallaxMode.TouchAndGyro)
                result += GetGyroOffset();

            result += GetAutoMotionOffset();
            return ClampPerAxis(ApplyInvert(result));
        }

        private Vector2 GetMouseOffset(float strengthX, float strengthY)
        {
            Vector2 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);

            if (screenCenter.x <= 0f || screenCenter.y <= 0f)
                return Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null)
                return Vector2.zero;

            Vector2 mousePosition = Mouse.current.position.ReadValue();
#else
            Vector2 mousePosition = Input.mousePosition;
#endif
            Vector2 delta = mousePosition - screenCenter;

            return new Vector2(
                (delta.x / screenCenter.x) * strengthX,
                (delta.y / screenCenter.y) * strengthY);
        }

        private Vector2 GetTouchOffsetOrZero()
        {
            if (!TryGetTouchPosition(out Vector2 touchPosition))
                return Vector2.zero;

            Vector2 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);

            if (screenCenter.x <= 0f || screenCenter.y <= 0f)
                return Vector2.zero;

            Vector2 delta = touchPosition - screenCenter;

            return new Vector2(
                (delta.x / screenCenter.x) * _touchStrengthX,
                (delta.y / screenCenter.y) * _touchStrengthY);
        }

        private Vector2 GetGyroOffset()
        {
            Vector3 acc = ReadAcceleration();

            Vector2 raw = new(
                acc.x * _gyroStrengthX,
                acc.y * _gyroStrengthY);

            _gyroSmoothed = Vector2.Lerp(
                _gyroSmoothed,
                raw,
                Time.unscaledDeltaTime * _gyroSmoothing);

            return _gyroSmoothed * _gyroMultiplier;
        }

        private static bool TryGetTouchPosition(out Vector2 position)
        {
#if ENABLE_INPUT_SYSTEM
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                foreach (var touch in touchscreen.touches)
                {
                    if (!touch.press.isPressed)
                        continue;

                    position = touch.position.ReadValue();
                    return true;
                }
            }
#else
            if (Input.touchCount > 0)
            {
                position = Input.GetTouch(0).position;
                return true;
            }
#endif
            position = Vector2.zero;
            return false;
        }

        private static Vector3 ReadAcceleration()
        {
#if ENABLE_INPUT_SYSTEM
            var accelerometer = Accelerometer.current;
            if (accelerometer == null)
                return Vector3.zero;

            // Sensors start disabled. Check on read to also handle device reconnection.
            // Do not disable this shared device when a parallax screen closes.
            if (!accelerometer.enabled)
                InputSystem.EnableDevice(accelerometer);

            return accelerometer.acceleration.ReadValue();
#else
            return Input.acceleration;
#endif
        }

        private Vector2 GetAutoMotionOffset()
        {
            return new Vector2(
                Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * _autoMotionFrequency.x) * _autoMotionAmplitude.x,
                Mathf.Cos(Time.unscaledTime * Mathf.PI * 2f * _autoMotionFrequency.y) * _autoMotionAmplitude.y);
        }

        private Vector2 ApplyInvert(Vector2 value)
        {
            if (_invertX)
                value.x *= -1f;

            if (_invertY)
                value.y *= -1f;

            return value;
        }

        private Vector2 ClampPerAxis(Vector2 value)
        {
            return new Vector2(
                Mathf.Clamp(value.x, -_maxOffset.x, _maxOffset.x),
                Mathf.Clamp(value.y, -_maxOffset.y, _maxOffset.y));
        }

        private void ApplyOffset(Vector2 offset, bool instant)
        {
            if (_layers == null)
                return;

            for (int i = 0; i < _layers.Length; i++)
            {
                if (_layers[i] != null)
                    _layers[i].SetOffset(offset, instant);
            }
        }

        public void ResetLayersToCurrentPosition()
        {
            if (_layers == null)
                return;

            for (int i = 0; i < _layers.Length; i++)
            {
                if (_layers[i] != null)
                    _layers[i].ResetLayer();
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
