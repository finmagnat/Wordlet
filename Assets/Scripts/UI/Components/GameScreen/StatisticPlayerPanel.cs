using System.Collections.Generic;
using Core.Services;
using Core.UI.Components;
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
        [SerializeField] private WordListItem _wordListItemPrefab;
        [SerializeField] private Image _mainBackground;
        [SerializeField] private Image _handleBackground;
        
        [Inject] private SkinsService _skinsService;
        [Inject] private ISpriteService _spritesService;
        [Inject] private LocalizationService _localization;
        
        /// <summary>
        /// Добавить слово.
        /// </summary>
        public async void AddWord(string value)
        {
            if (_wordListItemPrefab && _listContent)
            {
                CreateWordItem(value);
                
                await UniTask.Yield();
                
                _scrollRect.verticalNormalizedPosition = 0;
            }
        }
        
        public async void AddWords(List<string> words)
        {
            if (_wordListItemPrefab && _listContent)
            {
                foreach (var value in words)
                    CreateWordItem(value);

                await UniTask.Yield();
                
                _scrollRect.verticalNormalizedPosition = 0;
            }
        }

        public void Reset()
        {
            Words.Clear();
            if (_listContent)
                foreach (Transform child in _listContent.transform)
                    Destroy(child.gameObject);
        }
        
        public async UniTask UpdateSkin()
        {
            var skin = _skinsService.SkinCurrent;
            _mainBackground.sprite = await _spritesService.GetSpriteAsync(skin.FrameBackgroundAlias);
            _handleBackground.sprite = await _spritesService.GetSpriteAsync(skin.HandleBackgroundAlias);
        }

        private void CreateWordItem(string word)
        {
            WordListItem wordListItem = Instantiate(_wordListItemPrefab, _listContent);
            wordListItem.Initialize(word);
            Words.Add(word);
        }
    }
}