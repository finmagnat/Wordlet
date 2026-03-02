using System.Collections.Generic;
using Core.Services;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Components
{
    public class StatisticPlayerPanel : MonoBehaviour
    {
        public List<string> Words { get; private set; } = new();

        [SerializeField] private ScrollRect _scrollRect;
        [Tooltip("Ссылка на контейнер для списка слов")]
        [SerializeField] private Transform _listContent;
        [Tooltip("Ссылка на префаб с элементом текстового поля")]
        [SerializeField] private GameObject _wordListItemPrefab;
        [SerializeField] private Image _mainBackground;
        
        [Inject] private SkinsService _skinsService;
        [Inject] private ISpriteService _spritesService;
        [Inject] private LocalizationService _localization;
        
        private void Start()
        {
            //SetSkin();
        }
        
        /// <summary>
        /// Добавить слово.
        /// </summary>
        public async void AddWord(string value)
        {
            if (_wordListItemPrefab && _listContent)
            {
                GameObject wordListItem = Instantiate(_wordListItemPrefab, _listContent);
                
                TextMeshProUGUI itemWord = wordListItem.GetComponent<TextMeshProUGUI>();
                itemWord.text = $"{value.Length} {value}";
                Words.Add(value);
                
                //await UniTask.Delay(500);
                await UniTask.Yield();
                
                _scrollRect.verticalNormalizedPosition = 0;
            }
        }
        
        public async void AddWords(List<string> words)
        {
            if (_wordListItemPrefab && _listContent)
            {
                foreach (var value in words)
                {
                    GameObject wordListItem = Instantiate(_wordListItemPrefab, _listContent);

                    TextMeshProUGUI itemWord = wordListItem.GetComponent<TextMeshProUGUI>();
                    itemWord.text = $"{value.Length} {value}";
                    Words.Add(value);
                }

                //await UniTask.Delay(500);
                await UniTask.Yield();
                
                _scrollRect.verticalNormalizedPosition = 0;
            }
        }

        public void Reset()
        {
            Words.Clear();
            if (_listContent)
            {
                foreach (Transform child in _listContent.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        
        private async UniTask SetSkin()
        {
            var skin = _skinsService.SkinCurrent;
            _mainBackground.sprite = await _spritesService.GetSpriteAsync(skin.ListBackgroundAlias);
        }
    }
}