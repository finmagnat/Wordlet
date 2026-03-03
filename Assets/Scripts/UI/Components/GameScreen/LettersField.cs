using System.Collections.Generic;
using Core.DataDictionary;
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
        
        private List<KeyboardLetter> _items;
      
        public void InitField()
        {
            string alphabet = _dictionaryService.Alphabet;
            if(alphabet.Length == 0)
                return;
         
            char[] arrAlphabet = alphabet.ToCharArray();
            
            if(_items == null)
                _items = new List<KeyboardLetter>(alphabet.Length);
            else
                _items.ForEach(item => item.gameObject.SetActive(false));
            
            for (int i = 0; i < arrAlphabet.Length; ++i)
            {
                if (_items.Count > i && _items[i] != null)
                {
                    var item = _items[i].gameObject;
                    item.GetComponent<KeyboardLetter>().SetLetter(arrAlphabet[i].ToString());
                    item.SetActive(true);
                }
                else
                {
                    var clonePrefab = Instantiate(_dragbleLetterPrefab, transform);

                    var draggedLetter = clonePrefab.GetComponent<KeyboardLetter>();
                    draggedLetter.SetLetter(arrAlphabet[i].ToString());

                    _items.Add(draggedLetter);
                }
            }

            //SetSkin();
        }
        
        private async UniTask SetSkin()
        {
            var skin = _skinsService.SkinCurrent;
            var sprite = await _spriteService.GetSpriteAsync(skin.DragableLetterAlias);
            _items.ForEach(item =>
                item.gameObject.GetComponent<KeyboardLetter>().SetSkin(sprite)
            );
        }
    }
}