using UnityEngine;

namespace Core.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class PopupMaxWidthFitter : MonoBehaviour
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private float _maxWidth = 700f;
        [SerializeField] private float _horizontalMargin = 40f;
        [SerializeField] private bool _controlHeight = false;
        [SerializeField] private float _maxHeight = 1000f;
        [SerializeField] private float _verticalMargin = 40f;

        private void Reset()
        {
            _target = GetComponent<RectTransform>();
        }

        private void Awake()
        {
            if (_target == null)
                _target = GetComponent<RectTransform>();

            Apply();
        }

        private void OnEnable()
        {
            Apply();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_target == null)
                _target = GetComponent<RectTransform>();

            Apply();
        }
#endif

        private void OnRectTransformDimensionsChange()
        {
            Apply();
        }

        public void Apply()
        {
            if (_target == null)
                return;

            Canvas rootCanvas = _target.GetComponentInParent<Canvas>();
            if (rootCanvas == null)
                return;

            RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();
            if (canvasRect == null)
                return;

            float parentWidth = canvasRect.rect.width;
            float allowedWidth = Mathf.Min(_maxWidth, parentWidth - _horizontalMargin * 2f);
            allowedWidth = Mathf.Max(0f, allowedWidth);

            Vector2 size = _target.sizeDelta;
            size.x = allowedWidth;

            if (_controlHeight)
            {
                float parentHeight = canvasRect.rect.height;
                float allowedHeight = Mathf.Min(_maxHeight, parentHeight - _verticalMargin * 2f);
                allowedHeight = Mathf.Max(0f, allowedHeight);
                size.y = allowedHeight;
            }

            _target.sizeDelta = size;
        }
    }
}