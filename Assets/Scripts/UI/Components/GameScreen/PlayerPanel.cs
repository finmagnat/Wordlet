using System.Collections.Generic;
using Core.Services;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Components
{
    public class PlayerPanel : MonoBehaviour
    {
        public uint Score { get; private set; }
        public uint Pass { get; private set; }
        public List<string> Words { get; private set; } = new();

        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _passText;
        [Tooltip("Ссылка на контейнер для списка слов")]
        [SerializeField] private Transform _listContent;
        [Tooltip("Ссылка на префаб с элементом текстового поля")]
        [SerializeField] private GameObject _wordListItemPrefab;
        [SerializeField] private Image _mainBackground;
        
        [Inject] private SkinsService _skinsService;
        [Inject] private ISpriteService _spritesService;
        [Inject] private LocalizationService _localization;
        
        public uint _maxPasses;
        
        private void Start()
        {
            SetSkin();
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
                itemWord.text = value;
                Words.Add(value);
                
                await UniTask.Delay(500);
                
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
                    itemWord.text = value;
                    Words.Add(value);
                }

                await UniTask.Delay(500);
                
                _scrollRect.verticalNormalizedPosition = 0;
            }
        }

        public void SetPlayerName(string name) => _playerNameText.text = name;
        
        public void SetData(uint score = 0, uint pass = 0, uint maxPasses = 0)
        {
            _maxPasses = maxPasses;
            SetScore(score);
            SetPass(pass, maxPasses);
        }
        
        public void SetScore(uint score)
        {
            Score = score;            
            _scoreText.text = _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyLabelScore, score);
        }

        public void SetPass(uint pass, uint maxPasses)
        {
            Pass = pass;
            _passText.text = _localization.Get(LocalizationConst.TableUI, LocalizationConst.KeyLabelPasses, pass, maxPasses);
        }

        public void Reset()
        {
            Pass = 0;
            Score = 0;
            SetPlayerName("");
            SetData();
            Words.Clear();
            if (_listContent)
            {
                foreach (Transform child in _listContent.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        
        public void UpdateText()
        {
            SetData(Score, Pass, _maxPasses);
        }

        private async UniTask SetSkin()
        {
            var skin = _skinsService.SkinCurrent;
            _mainBackground.sprite = await _spritesService.GetSpriteAsync(skin.ListBackgroundAlias);
        }
    }
}