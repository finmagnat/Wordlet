using Core.Events;
using Core.Services;
using Cysharp.Threading.Tasks;
using UI.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Components
{
    public class KeyboardPanel : UIPopup
    {
        [SerializeField] private Image _mainBackground;
        
        [Inject] private SkinsService _skinsService;
        [Inject] private ISpriteService _spritesService;
        [Inject] private LocalizationService _localization;
        
        private void Start()
        {
            EventBus.Subscribe<KeyboardLetterSelectEvent>(OnKeyBoardLetterSelect);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<KeyboardLetterSelectEvent>(OnKeyBoardLetterSelect);
        }
        
        public async UniTask UpdateSkin()
        {
            var skin = _skinsService.SkinCurrent;
            _mainBackground.sprite = await _spritesService.GetSpriteAsync(skin.FrameBackgroundAlias);
        }

        private void OnKeyBoardLetterSelect(KeyboardLetterSelectEvent obj)
        {
            HideAsync().Forget();
        }
    }
}