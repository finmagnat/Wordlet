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
        public string PlayerName => _playerNameText.text;
        public List<string> Words { get; private set; } = new(); // TODO: перенести в панель статистики

        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _passText;
        [SerializeField] private Image _mainBackground;
        
        [Inject] private SkinsService _skinsService;
        [Inject] private ISpriteService _spritesService;
        [Inject] private LocalizationService _localization;
        
        private void Start()
        {
            //SetSkin();
        }
        
        public void SetPlayerName(string name) => _playerNameText.text = name;
        
        public void SetData(uint score = 0, uint pass = 0, uint maxPasses = 0)
        {
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
        }
        
        private async UniTask SetSkin()
        {
            var skin = _skinsService.SkinCurrent;
            _mainBackground.sprite = await _spritesService.GetSpriteAsync(skin.ListBackgroundAlias);
        }
    }
}