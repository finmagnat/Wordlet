using System.Collections.Generic;
using Core.Data;
using Core.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace UI.Components
{
    public class WordsField : MonoBehaviour
    {
        [SerializeField] private GameObject _selectebleLetterPrefab;
     
        [Inject] private SkinsService _skinsService;
        [Inject] private ISpriteService _spritesService;
        
        private List<SelectableLetter> _items = new (WordsFieldData.AMOUNT_LETTERS);
        private bool _isInitialized;
        public List<SelectableLetter> InitField()
        {
            if (!_isInitialized)
            {
                for (int i = 0; i < WordsFieldData.AMOUNT_LETTERS; ++i)
                {
                    var clonePrefab = Instantiate(_selectebleLetterPrefab);
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
        
        private async UniTask SetSkin()
        {
            var skin = _skinsService.SkinCurrent;
            var sprite = await _spritesService.GetSpriteAsync(skin.SelectableLetterAlias);
            _items.ForEach(item => item.SetSkin(sprite));
        }
    }
}