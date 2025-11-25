using Core.Events;
using UnityEngine;

namespace UI.Components
{
    public class DragAndDropBehaviour : MonoBehaviour
    {
        private Vector3 _dragOffset;
        private Camera _camera;
        private Vector3 _pos;
        private bool _bMove = false;
        
        [SerializeField] private float _speed = 1000;

        public void SetEnable (bool value = true) => _bMove = value;
        public void SetCamera(Camera camera) => _camera = camera;
        
        void OnMouseDown()
        {
            if (!_bMove) return;
            _pos = transform.position;            
            _dragOffset = _pos - GetMousePos();
            //Debug.Log(_pos);
        }

        void OnMouseDrag()
        {
            if (!_bMove) return;
            transform.position = Vector3.MoveTowards(transform.position, GetMousePos() + _dragOffset, _speed * Time.deltaTime);            
        }

        void OnMouseUp()
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

        Vector3 GetMousePos()
        {
            var mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            return mousePos;
        }
    }
}