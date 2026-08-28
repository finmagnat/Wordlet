using Core.Services;
using Cysharp.Threading.Tasks;
using UI.Components;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Popups
{
    public class FloatingPausePopup : UIPopup
    {
        [Header("References")]
        [SerializeField] protected PauseButtonAnimator _pauseButtonAnimator;
        [SerializeField] protected Image _pauseImage;
        
        [Inject] private SkinsService _skinsService;
        [Inject] private ISpriteService _spritesService;
        
        public override async UniTask ShowAsync()
        {
            await base.ShowAsync();
            _pauseButtonAnimator.SetPaused(true);
        }

        public override async UniTask HideAsync()
        {
            await base.HideAsync();
            _pauseButtonAnimator.SetPaused(false);
        }
        
        internal async UniTask UpdateSkin()
        {
            var skin = _skinsService.SkinCurrent;
            _pauseImage.sprite = await _spritesService.GetSpriteAsync(skin.PauseButtonAlias);
        }

    }
}
