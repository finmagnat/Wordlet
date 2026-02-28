using System.Collections.Generic;
using Core.DataDictionary;
using Core.Events;
using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace UI.Components
{
    public class LettersField : MonoBehaviour
    {
        [SerializeField] private GameObject _dragbleLetterPrefab;

        [Inject] private SkinsService _skinsService;
        [Inject] private ISpriteService _spriteService;
        [Inject] private LocalizationService _localization;
        [Inject] private DictionaryService _dictionaryService;
        
        private List<DraggedLetter> _items;
      
        private void Start()
        {
            EventBus.Subscribe<GameEndEvent>(OnGameEnd);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<GameEndEvent>(OnGameEnd);
        }
        
        public List<DraggedLetter> InitField()
        {
            string alphabet = _dictionaryService.Alphabet;
            if(alphabet.Length == 0)
                return null;
         
            char[] arrAlphabet = alphabet.ToCharArray();
            
            if(_items == null)
                _items = new List<DraggedLetter>(alphabet.Length);
            else
                _items.ForEach(item => item.gameObject.SetActive(false));
            
            for (int i = 0; i < arrAlphabet.Length; ++i)
            {
                if (_items.Count > i && _items[i] != null)
                {
                    var item = _items[i].gameObject;
                    item.GetComponent<DraggedLetter>().SetLetter(arrAlphabet[i].ToString());
                    item.SetActive(true);
                }
                else
                {
                    var clonePrefab = Instantiate(_dragbleLetterPrefab, transform);

                    var draggedLetter = clonePrefab.GetComponent<DraggedLetter>();
                    draggedLetter.SetLetter(arrAlphabet[i].ToString());

                    _items.Add(draggedLetter);
                }
            }

            //SetSkin();
            
            return _items;
        }
        
        public void SetEnable(bool value = true) => _items.ForEach(item => item.SetEnable(value));
        
        private async UniTask SetSkin()
        {
            var skin = _skinsService.SkinCurrent;
            var sprite = await _spriteService.GetSpriteAsync(skin.DragableLetterAlias);
            _items.ForEach(item =>
                item.gameObject.GetComponent<DraggedLetter>().SetSkin(sprite)
            );
        }
        
        private void OnGameEnd(GameEndEvent eventData)
        {
            SetEnable(false);
        }
    }
}