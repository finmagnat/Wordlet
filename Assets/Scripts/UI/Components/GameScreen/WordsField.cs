using System.Collections.Generic;
using Core.Data;
using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using Core.Events;

namespace UI.Components
{
    public class WordsField : MonoBehaviour
    {
        [SerializeField] private GameObject _selectebleLetterPrefab;
     
        [Inject] private SkinsService _skinsService;
        [Inject] private ISpriteService _spritesService;
        [Inject] private DiContainer _container;
        
        private const int COLS = 5;

        private bool _isDragging;
        private readonly List<SelectableLetter> _dragPath = new();
        private readonly HashSet<int> _dragVisited = new();
        
        private List<SelectableLetter> _items = new (WordsFieldData.AMOUNT_LETTERS);
        private bool _isInitialized;
        
        public List<SelectableLetter> InitField()
        {
            _isDragging = false;
            _dragPath.Clear();
            _dragVisited.Clear();
            
            if (!_isInitialized)
            {
                for (int i = 0; i < WordsFieldData.AMOUNT_LETTERS; ++i)
                {
                    var clonePrefab = _container.InstantiatePrefab(_selectebleLetterPrefab);
                    clonePrefab.transform.SetParent(transform);
                    clonePrefab.transform.localScale = new Vector3(1, 1, 1);

                    var letter = clonePrefab.GetComponent<SelectableLetter>();
                    letter.SetLetter("");
                    letter.Index = i;

                    _items.Add(letter);
                }
                _isInitialized = true;
            }
            else
            {
                _items.ForEach(item => item.Reset());
            }

            SetSkin();
            
            return _items;
        }

        public void BeginDragSelection(SelectableLetter start)
        {
            if (start == null || start.Empty())
                return;

            _isDragging = true;
            _dragPath.Clear();
            _dragVisited.Clear();

            // первая буква — всегда попытка выбрать
            AddToPathAndNotifySelect(start);
        }

        public void ContinueDragSelection(SelectableLetter current)
        {
            if (!_isDragging || current == null || current.Empty())
                return;

            // ШАГ НАЗАД: зашли на предпоследнюю букву => снимаем последнюю
            if (_dragPath.Count >= 2 && current == _dragPath[^2])
            {
                RemoveLastFromPathAndNotifyBacktrack();
                return;
            }

            // Нельзя "кусать себя" (любое другое повторное посещение запрещено)
            if (_dragVisited.Contains(current.Index))
                return;

            // Если это не первая — проверяем ортогонального соседа (без диагоналей)
            if (_dragPath.Count > 0)
            {
                var last = _dragPath[^1];
                if (!AreOrthogonalNeighbours(last.Index, current.Index))
                    return;
            }

            AddToPathAndNotifySelect(current);
        }

        public void EndDragSelection()
        {
            _isDragging = false;
        }

        private void AddToPathAndNotifySelect(SelectableLetter letter)
        {
            if (!_dragVisited.Add(letter.Index))
                return;

            _dragPath.Add(letter);

            // всё остальное (валидность по игровым правилам, Highlight, добавление буквы в слово)
            // делает WordsFieldManager через существующий LetterSelectEvent
            EventBus.Raise(new LetterSelectEvent { letter = letter });
        }

        private void RemoveLastFromPathAndNotifyBacktrack()
        {
            if (_dragPath.Count == 0)
                return;

            // локально откатываем маршрут
            var last = _dragPath[^1];
            _dragPath.RemoveAt(_dragPath.Count - 1);
            _dragVisited.Remove(last.Index);

            // дальше пусть менеджер снимет Highlight и скажет UI удалить последний символ
            EventBus.Raise(new LetterBacktrackEvent());
        }

        private bool AreOrthogonalNeighbours(int aIndex, int bIndex)
        {
            int ax = aIndex % COLS;
            int ay = aIndex / COLS;
            int bx = bIndex % COLS;
            int by = bIndex / COLS;

            int dx = Mathf.Abs(ax - bx);
            int dy = Mathf.Abs(ay - by);

            return dx + dy == 1;
        }

        private async UniTask SetSkin()
        {
            var skin = _skinsService.SkinCurrent;
            var sprite = await _spritesService.GetSpriteAsync(skin.SelectableLetterAlias);
            _items.ForEach(item => item.SetSkin(sprite));
        }
    }
}