using Core.Events;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Components
{
    public class DraggedLetter : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private TextMeshProUGUI _letterText;
        [SerializeField] private Image _mainBackground;
        [SerializeField] private float _speed = 1000f;

        private RectTransform _rect;
        private Canvas _canvas;
        private Vector2 _pointerOffset;
        private Vector3 _startPos;
        private bool _bMove;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _canvas = GetComponentInParent<Canvas>();
        }

        private void Start()
        {
            EventBus.Subscribe<DragAndDropEvent>(OnDragAndDropFinishEvent);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<DragAndDropEvent>(OnDragAndDropFinishEvent);
        }

        public void SetLetter(string letter) => _letterText.text = letter;
        public void SetEnable(bool value = true) => _bMove = value;
        public void SetSkin(Sprite sprite) => _mainBackground.sprite = sprite;

        private void OnDragAndDropFinishEvent(DragAndDropEvent eventData)
        {
            if (eventData.gameObject == gameObject)
                EventBus.Raise(new LetterReleaseEvent { letter = _letterText.text, position = eventData.position });
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_bMove) return;

            _startPos = _rect.position;

            // вычисляем смещение относительно указателя
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_canvas.transform,
                eventData.position,
                null, // Overlay -> camera не нужна
                out var localPoint);

            // offset = текущая позиция буквы в локальных координатах Canvas - позиция указателя
            var currentLocal = (Vector2)_canvas.transform.InverseTransformPoint(_rect.position);
            _pointerOffset = currentLocal - localPoint;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_bMove) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_canvas.transform,
                eventData.position,
                null,
                out var localPoint);

            var targetLocal = localPoint + _pointerOffset;
            var targetWorld = _canvas.transform.TransformPoint(targetLocal);

            _rect.position = Vector3.MoveTowards(_rect.position, targetWorld, _speed * Time.deltaTime);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_bMove)
            {
                if (_rect.position != _startPos && _startPos != Vector3.zero)
                    _rect.position = _startPos;
                return;
            }

            EventBus.Raise(new DragAndDropEvent { gameObject = gameObject, position = _rect.position });
            _rect.position = _startPos;
        }
    }
}
