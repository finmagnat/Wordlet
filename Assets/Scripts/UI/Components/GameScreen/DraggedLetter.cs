using Core.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    public class DraggedLetter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _letterText;
        [SerializeField] private Image _mainBackground;
        [SerializeField] private float _speed = 1000;
        
        private Vector3 _dragOffset;
        private Camera _camera;
        private Vector3 _pos;
        private bool _bMove = false;
        
        private void Start()
        {
            EventBus.Subscribe<DragAndDropEvent>(OnDragAndDropFinishEvent);
        }
        
        private void OnDestroy()
        {
            EventBus.Unsubscribe<DragAndDropEvent>(OnDragAndDropFinishEvent);
        }
        
        public void SetLetter(string letter) => _letterText.text = letter;
        public void SetEnable (bool value = true) => _bMove = value;
        public void SetCamera(Camera camera) => _camera = camera;
        public void SetSkin(Sprite sprite) => _mainBackground.sprite = sprite;

        private void OnDragAndDropFinishEvent(DragAndDropEvent eventData)
        {
            if (eventData.gameObject == transform.gameObject)
            {
                EventBus.Raise(new LetterReleaseEvent{letter = _letterText.text, position = eventData.position});
            }            
        }
        
        private void OnMouseDown()
        {
            if (!_bMove) return;
            _pos = transform.position;            
            _dragOffset = _pos - GetMousePos();
            //Debug.Log(_pos);
        }

        private void OnMouseDrag()
        {
            if (!_bMove) return;
            transform.position = Vector3.MoveTowards(transform.position, GetMousePos() + _dragOffset, _speed * Time.deltaTime);            
        }

        private void OnMouseUp()
        {
            if (!_bMove)
            {
                if(transform.position != _pos && _pos != Vector3.zero)
                    transform.position = _pos;
                return;
            }
            
            EventBus.Raise(new DragAndDropEvent {gameObject = transform.gameObject, position = transform.position});
            
            transform.position = _pos;
        }

        private Vector3 GetMousePos()
        {
            var mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            return mousePos;
        }
    }
}