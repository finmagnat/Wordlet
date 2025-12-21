using System.Collections.Generic;
using Core.Events;
using UI.Components;

namespace Game.Logic
{
    public class LettersFieldManager
    {
        private List<DraggedLetter> _items;

        public void Initialize()
        {
            EventBus.Subscribe<GameEndEvent>(OnGameEnd);
        }

        public void Destroy()
        {
            EventBus.Unsubscribe<GameEndEvent>(OnGameEnd);
        }
        
        public void Init(List<DraggedLetter> items)
        {
            _items = items;
        }

        public void SetEnable(bool value = true) => _items.ForEach(item => item.SetEnable(value));
        
        private void OnGameEnd(GameEndEvent eventData)
        {
            SetEnable(false);
        }
    }
}