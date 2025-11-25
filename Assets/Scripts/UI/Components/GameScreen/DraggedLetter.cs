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

        public void SetLetter(string letter) => _letterText.text = letter;
        
        private void Start()
        {
            EventBus.Subscribe<DragAndDropEvent>(OnDragAndDropFinishEvent);
        }
        
        private void OnDestroy()
        {
            EventBus.Unsubscribe<DragAndDropEvent>(OnDragAndDropFinishEvent);
        }
        
        public void SetSkin(Sprite sprite) => _mainBackground.sprite = sprite;

        private void OnDragAndDropFinishEvent(DragAndDropEvent eventData)
        {
            if (eventData.gameObject == transform.gameObject)
            {
                EventBus.Raise(new LetterReleaseEvent{letter = _letterText.text, position = eventData.position});
            }            
        }
    }
}