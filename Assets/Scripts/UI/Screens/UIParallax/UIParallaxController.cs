using UnityEngine;

namespace UI.Parallax
{
    public sealed class UIParallaxController : MonoBehaviour
    {
        [SerializeField] private UIParallaxLayer[] _layers;

        [Header("Input")]
        [SerializeField] private bool _invertX;
        [SerializeField] private bool _invertY;

        [Header("Auto Motion")]
        [SerializeField] private bool _useAutoMotionWhenNoInput = true;
        [SerializeField] private Vector2 _autoMotionAmplitude = new(0.25f, 0.15f);
        [SerializeField] private Vector2 _autoMotionFrequency = new(0.05f, 0.08f);

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
            return GetMouseOffset();
#elif UNITY_ANDROID || UNITY_IOS
            Vector2 touchOffset = GetTouchOffset();
            if (touchOffset != Vector2.zero)
                return touchOffset;

            if (_useAutoMotionWhenNoInput)
                return GetAutoMotionOffset();

            return Vector2.zero;
#else
            if (_useAutoMotionWhenNoInput)
                return GetAutoMotionOffset();

            return Vector2.zero;
#endif
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

        private Vector2 GetTouchOffset()
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

            return ApplyInvert(normalized);
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
    

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_layers == null)
                return;

            for (int i = 0; i < _layers.Length; i++)
            {
                if (_layers[i] == null)
                    continue;
            }
        }
#endif
    }
}