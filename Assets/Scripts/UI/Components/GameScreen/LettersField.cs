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
        [SerializeField] private KeyboardLetter _keyboardLetterPrefab;
        [SerializeField] private AdaptiveKeyboardGrid _adaptiveKeyboardGrid;

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
                    var item = _items[i];
                    item.SetLetter(arrAlphabet[i].ToString());
                    item.gameObject.SetActive(true);
                }
                else
                {
                    var keyboardLetter = Instantiate(_keyboardLetterPrefab, transform);
                    keyboardLetter.SetLetter(arrAlphabet[i].ToString());
                    _items.Add(keyboardLetter);
                }
            }
            
            _adaptiveKeyboardGrid.RefreshLayout(alphabet.Length);

            SetSkin();
        }
        
        private async UniTask SetSkin()
        {
            var skin = _skinsService.SkinCurrent;
            var keyboardTile = await _spriteService.GetSpriteAsync(skin.KeyboardTileAlias);
            _items.ForEach(item =>
                item.SetSkin(keyboardTile, skin.KeyboardLetterColor)
            );
        }
    }
}